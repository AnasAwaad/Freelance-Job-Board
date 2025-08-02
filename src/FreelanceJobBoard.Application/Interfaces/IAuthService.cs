using FreelanceJobBoard.Application.DTOs;
using FreelanceJobBoard.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreelanceJobBoard.Application.Interfaces;
public interface IAuthService
{
    Task<AuthResponseDto> LoginAsync(string email, string password);



    Task RegisterFreelancerAsync(string email, string password, string fullName);
    Task RegisterClientAsync(string email, string password, string fullName);

}