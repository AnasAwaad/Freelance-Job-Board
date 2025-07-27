using FreelanceJobBoard.Application.Features.Auth.Commands.FreelancerRegister;
using FreelanceJobBoard.Application.Interfaces;
using FreelanceJobBoard.Domain.Constants;
using FreelanceJobBoard.Domain.Entities;
using FreelanceJobBoard.Domain.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreelanceJobBoard.Application.Features.Auth.Commands.FreelancerRegister;

internal class FreelancerRegisterCommandHandler(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IUnitOfWork unitOfWork) : IRequestHandler<FreelancerRegisterCommand>
{
    public async Task Handle(FreelancerRegisterCommand request, CancellationToken cancellationToken)
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
        if (!await roleManager.RoleExistsAsync(AppRoles.Freelancer))
        {
            await roleManager.CreateAsync(new IdentityRole(AppRoles.Freelancer));
        }

        // Add user to role
        await userManager.AddToRoleAsync(user, AppRoles.Freelancer);


        var Freelancer = new Freelancer
        {
            UserId = user.Id,
            AverageRating = 0,
            Bio = "",
            TotalReviews = 0,
            IsActive = true,
            CreatedOn = DateTime.UtcNow,
            Proposals = new List<Proposal>(),
            Contracts = new List<Contract>()
        };

        await unitOfWork.Freelancers.CreateAsync(Freelancer);
        await unitOfWork.SaveChangesAsync();

    }
}

