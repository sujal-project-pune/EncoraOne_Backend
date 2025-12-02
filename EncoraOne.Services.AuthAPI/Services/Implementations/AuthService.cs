using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using EncoraOne.Grievance.API.DTOs;
using EncoraOne.Grievance.API.Models;
using EncoraOne.Grievance.API.Repositories.Interfaces;
using EncoraOne.Grievance.API.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;

namespace EncoraOne.Grievance.API.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;

        public AuthService(IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterUserDto registerDto)
        {
            if (await UserExistsAsync(registerDto.Email))
            {
                throw new Exception("User with this email already exists.");
            }

            // HashPassword now returns string correctly
            string storedPassword = HashPassword(registerDto.Password);

            User newUser;

            // 1. Handle Manager
            if (registerDto.Role == UserRole.Manager)
            {
                if (!registerDto.DepartmentId.HasValue)
                    throw new Exception("Department ID is required for Managers.");

                newUser = new Manager
                {
                    DepartmentId = registerDto.DepartmentId.Value,
                    JobTitle = registerDto.JobTitle ?? "Manager"
                };
            }
            // 2. Handle Employee
            else if (registerDto.Role == UserRole.Employee)
            {
                newUser = new Employee
                {
                    JobTitle = registerDto.JobTitle ?? "Staff"
                };
            }
            // 3. Handle Admin (FIXED: Added JobTitle)
            else
            {
                // Admin is stored as a Manager with DeptId 1 (Administration)
                newUser = new Manager
                {
                    DepartmentId = 1,
                    // FIX: Assign JobTitle to prevent SQL Null Error
                    JobTitle = registerDto.JobTitle ?? "System Administrator"
                };
            }

            newUser.FullName = registerDto.FullName;
            newUser.Email = registerDto.Email;
            newUser.PasswordHash = storedPassword;
            newUser.Role = registerDto.Role;
            newUser.IsActive = true;

            if (newUser is Manager m) await _unitOfWork.Managers.AddAsync(m);
            else if (newUser is Employee e) await _unitOfWork.Employees.AddAsync(e);

            await _unitOfWork.CompleteAsync();

            return GenerateTokenResponse(newUser);
        }

        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto loginDto)
        {
            var employees = await _unitOfWork.Employees.FindAsync(u => u.Email == loginDto.Email);
            User user = employees.FirstOrDefault();

            if (user == null)
            {
                var managers = await _unitOfWork.Managers.FindAsync(u => u.Email == loginDto.Email);
                user = managers.FirstOrDefault();
            }

            if (user == null) throw new Exception("Invalid Credentials");

            if (!VerifyPasswordHash(loginDto.Password, user.PasswordHash))
            {
                throw new Exception("Invalid Credentials");
            }

            return GenerateTokenResponse(user);
        }

        public async Task<bool> UserExistsAsync(string email)
        {
            var emp = await _unitOfWork.Employees.FindAsync(e => e.Email == email);
            if (emp.Any()) return true;

            var mgr = await _unitOfWork.Managers.FindAsync(m => m.Email == email);
            if (mgr.Any()) return true;

            return false;
        }

        public async Task<string> ForgotPasswordAsync(string email)
        {
            var emp = (await _unitOfWork.Employees.FindAsync(e => e.Email == email)).FirstOrDefault();
            User user = emp;

            if (user == null)
            {
                var mgr = (await _unitOfWork.Managers.FindAsync(m => m.Email == email)).FirstOrDefault();
                user = mgr;
            }

            if (user == null) throw new Exception("User not found.");

            string otp = new Random().Next(100000, 999999).ToString();

            user.ResetToken = otp;
            user.ResetTokenExpires = DateTime.UtcNow.AddMinutes(15);

            if (user is Employee e) _unitOfWork.Employees.Update(e);
            else if (user is Manager m) _unitOfWork.Managers.Update(m);

            await _unitOfWork.CompleteAsync();

            return otp;
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordDto resetDto)
        {
            var emp = (await _unitOfWork.Employees.FindAsync(u => u.Email == resetDto.Email)).FirstOrDefault();
            User user = emp;

            if (user == null)
            {
                var mgr = (await _unitOfWork.Managers.FindAsync(u => u.Email == resetDto.Email)).FirstOrDefault();
                user = mgr;
            }

            if (user == null) throw new Exception("User not found.");

            if (user.ResetToken != resetDto.Otp) throw new Exception("Invalid OTP.");
            if (!user.ResetTokenExpires.HasValue || user.ResetTokenExpires < DateTime.UtcNow)
                throw new Exception("OTP has expired.");

            user.PasswordHash = HashPassword(resetDto.NewPassword);

            user.ResetToken = null;
            user.ResetTokenExpires = null;

            if (user is Employee e) _unitOfWork.Employees.Update(e);
            else if (user is Manager m) _unitOfWork.Managers.Update(m);

            await _unitOfWork.CompleteAsync();

            return true;
        }

        // FIX: Changed return type from object to string
        public string HashPassword(string password)
        {
            using var hmac = new HMACSHA512();

            byte[] computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

            string passwordHash = Convert.ToBase64String(computedHash);
            string passwordSalt = Convert.ToBase64String(hmac.Key);

            return $"{passwordSalt}.{passwordHash}";
        }

        private bool VerifyPasswordHash(string password, string storedPassword)
        {
            var parts = storedPassword.Split('.');
            if (parts.Length != 2) return false;

            var salt = Convert.FromBase64String(parts[0]);
            var hash = parts[1];

            using var hmac = new HMACSHA512(salt);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(computedHash) == hash;
        }

        private AuthResponseDto GenerateTokenResponse(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["JwtSettings:Key"]);

            int? deptId = (user as Manager)?.DepartmentId;

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim("role", user.Role.ToString())
            };

            if (deptId.HasValue)
            {
                claims.Add(new Claim("DepartmentId", deptId.Value.ToString()));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(double.Parse(_configuration["JwtSettings:DurationInMinutes"])),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _configuration["JwtSettings:Issuer"],
                Audience = _configuration["JwtSettings:Audience"]
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return new AuthResponseDto
            {
                Token = tokenHandler.WriteToken(token),
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role.ToString(),
                UserId = user.Id,
                DepartmentId = deptId
            };
        }
    }
}