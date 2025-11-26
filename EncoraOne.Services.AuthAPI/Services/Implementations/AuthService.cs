using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq; // Added for Linq
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

            using var hmac = new HMACSHA512();
            string passwordHash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)));
            string passwordSalt = Convert.ToBase64String(hmac.Key);
            string storedPassword = $"{passwordSalt}.{passwordHash}";

            User newUser;

            if (registerDto.Role == UserRole.Manager)
            {
                if (!registerDto.DepartmentId.HasValue)
                    throw new Exception("Department ID is required for Managers.");

                newUser = new Manager
                {
                    DepartmentId = registerDto.DepartmentId.Value
                };
            }
            else if (registerDto.Role == UserRole.Employee)
            {
                newUser = new Employee
                {
                    JobTitle = registerDto.JobTitle ?? "Staff"
                };
            }
            else
            {
                newUser = new Manager { DepartmentId = 1 }; // Admin default
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

            // Get Dept ID if Manager
            int? deptId = (user as Manager)?.DepartmentId;

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                // Add BOTH standard role claim types to be safe
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