using AutoMapper;
using FreelanceJobBoard.Application.Interfaces;
using FreelanceJobBoard.Domain.Entities;
using FreelanceJobBoard.Domain.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;
using FreelanceJobBoard.Domain.Constants;

namespace FreelanceJobBoard.Application.Features.Auth.Commands.ClientRegister;
internal class ClientRegisterCommandHandler(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IUnitOfWork unitOfWork) : IRequestHandler<ClientRegisterCommand>
{
    public async Task Handle(ClientRegisterCommand request, CancellationToken cancellationToken)
    {

        var existingUser = await userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            throw new InvalidOperationException("User with this email already exists");
        }

        // Create user
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FullName = request.FullName,
            EmailConfirmed = true,
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException($"User creation failed: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }

        // Ensure role exists
        if (!await roleManager.RoleExistsAsync(AppRoles.Client))
        {
            await roleManager.CreateAsync(new IdentityRole(AppRoles.Client));
        }

        // Add user to role
        await userManager.AddToRoleAsync(user, AppRoles.Client);

        // Create role-specific entities
        var company = new Company
        {
            Name = request.CompanyName ?? request.FullName,
            Description = request.CompanyDescription,
            Industry = request.CompanyIndustry
        };

        var client = new Client
        {
            UserId = user.Id,
            Company = company,
            AverageRating = 0,
            TotalReviews = 0,
            IsActive = true,
            CreatedOn = DateTime.UtcNow,
            Jobs = new List<Job>(),
            Proposals = new List<Proposal>(),
            Contracts = new List<Contract>()
        };

        await unitOfWork.Clients.CreateAsync(client);
        await unitOfWork.SaveChangesAsync();

    }
}
