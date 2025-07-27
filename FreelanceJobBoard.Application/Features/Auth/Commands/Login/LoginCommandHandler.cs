//using MediatR;
//using FreelanceJobBoard.Application.DTOs;
//using FreelanceJobBoard.Application.Interfaces;
//using FreelanceJobBoard.Application.Interfaces.Repositories;
//using AutoMapper;
//using FreelanceJobBoard.Application.Features.Auth.DTOs;
//using FreelanceJobBoard.Domain.Identity;
//using Microsoft.AspNetCore.Identity;

//namespace FreelanceJobBoard.Application.Features.Auth.Commands.Login;

//internal class LoginCommandHandler(
//    UserManager<ApplicationUser> userManager,
//    SignInManager<ApplicationUser> signInManager,
//    IJwtTokenGenerator jwtTokenGenerator,
//    IClientRepository clientRepository,
//    IFreelancerRepository freelancerRepository,
//    IMapper mapper) : IRequestHandler<LoginCommand, AuthResponseDto>
//{
//    public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
//    {
//        var dto = request.LoginDto;

//        var user = await userManager.FindByEmailAsync(dto.Email);
//        if (user == null)
//        {
//            throw new UnauthorizedAccessException("Invalid email or password");
//        }

//        var result = await signInManager.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: false);
//        if (!result.Succeeded)
//        {
//            throw new UnauthorizedAccessException("Invalid email or password");
//        }

//        var roles = await userManager.GetRolesAsync(user);
//        var token = jwtTokenGenerator.GenerateToken(user, roles);
//        var refreshToken = jwtTokenGenerator.GenerateRefreshToken();

//        var userInfo = mapper.Map<UserInfoDto>(user);
//        userInfo.Roles = roles.ToList();

//        // Get role-specific IDs
//        if (roles.Contains("Client"))
//        {
//            var client = await clientRepository.GetByUserIdAsync(user.Id);
//            userInfo.ClientId = client?.Id;
//        }
//        else if (roles.Contains("Freelancer"))
//        {
//            var freelancer = await freelancerRepository.GetByUserIdAsync(user.Id);
//            userInfo.FreelancerId = freelancer?.Id;
//        }

//        return new AuthResponseDto
//        {
//            Token = token,
//            RefreshToken = refreshToken,
//            ExpiresAt = DateTime.UtcNow.AddHours(24),
//            User = userInfo
//        };
//    }
//}