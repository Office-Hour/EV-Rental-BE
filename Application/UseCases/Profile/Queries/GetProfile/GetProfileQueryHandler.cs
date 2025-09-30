using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.CustomExceptions;
using Application.DTOs;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.UseCases.Profile.Queries.GetProfile
{
    public class GetProfileQueryHandler(UserManager<IdentityUser> userManager, IMapper mapper) : IRequestHandler<GetProfileQuery, UserInfoDto>
    {
        public async Task<UserInfoDto> Handle(GetProfileQuery request, CancellationToken cancellationToken)
        {
            // Get user by ID, throw exception if not found
            var user = await userManager.FindByIdAsync(request.UserId) ?? throw new UserNotFoundException(request.UserId);
            // Map user to UserInfoDto
            var userInfo = mapper.Map<UserInfoDto>(user);
            return userInfo;
        }
    }
}
