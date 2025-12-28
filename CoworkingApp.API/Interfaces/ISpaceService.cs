using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CoworkingApp.API.DTOs;

namespace CoworkingApp.API.Interfaces
{
    public interface ISpaceService
    {
        Task<SpaceResponse> CreateSpaceAsync(CreateSpaceRequest request);
        Task<SpaceResponse> GetSpaceByIdAsync(Guid id);
        Task<List<SpaceResponse>> SearchSpacesAsync(SpaceSearchRequest request);
        Task<SpaceResponse> UpdateSpaceAsync(Guid id, UpdateSpaceRequest request); // <--- NEW
        Task<bool> DeleteSpaceAsync(Guid id);
        Task<List<LocationDto>> GetAllLocationsAsync();
    }
}