#nullable enable
using STO.Library.Domain.Models.Users;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace STO.Identity.Services.Interfaces
{
    public interface ICompanyUserService
    {
        Task<CompanyUserRoleResponse> GetValidCompanyUserRoles(Guid userId, string inputCompanyCode);
        Task<List<CompanyUserRole>> GetCompanyUserRoles(Guid userId, string? companyCode);
        Task<List<CompanyUserRole>> GetCompanyUserRoles(Guid userId);
        //Task<CompanyUserRoleEntity> AddNewCompanyUserRole(CompanyUserRoleEntity companyUserRole);
        //Task<CompanyUserRoleEntity> UpdateCompanyUserRole(CompanyUserRoleEntity companyUserRole);
        //Task<List<GetCompaniesResponse>> GetCompaniesByUserId(Guid userId);
        Task<List<CompanyUserRoleResponse>> GetActiveCompanyUserRoles(Guid userId);
        Task<List<CompanyUser>> GetCompanyUsers(string companyCode);
    }
}