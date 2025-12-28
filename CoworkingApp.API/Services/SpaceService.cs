using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CoworkingApp.API.Data;
using CoworkingApp.API.DTOs;
using CoworkingApp.API.Enums;
using CoworkingApp.API.Interfaces;
using CoworkingApp.API.Models;
using CoworkingApp.API.Exceptions;

namespace CoworkingApp.API.Services
{
    public class SpaceService : ISpaceService
    {
        private readonly CoworkingContext _context;

        public SpaceService(CoworkingContext context)
        {
            _context = context;
        }

        public async Task<SpaceResponse> CreateSpaceAsync(CreateSpaceRequest request)
        {
            // 1. Validate Location
            var location = await _context.Locations.FindAsync(request.LocationId);
            if (location == null) throw new BadRequestException("Location not found.");

            // 2. Map DTO to Model
            var space = new Space
            {
                SpaceId = Guid.NewGuid(),
                Name = request.Name,
                Type = request.Type,
                Capacity = request.Capacity,
                Floor = request.Floor,
                Area = request.Area,
                Description = request.Description,
                PricePerHour = request.PricePerHour,
                Status = SpaceStatus.AVAILABLE,
                LocationId = request.LocationId,
                Amenities = new List<Amenity>()
            };

            // 3. Link Amenities
            if (request.AmenityIds != null && request.AmenityIds.Any())
            {
                var amenities = await _context.Amenities
                    .Where(a => request.AmenityIds.Contains(a.AmenityId))
                    .ToListAsync();
                
                foreach (var amenity in amenities)
                {
                    space.Amenities.Add(amenity);
                }
            }

            // 4. Save to DB
            _context.Spaces.Add(space);
            await _context.SaveChangesAsync();

            return MapToResponse(space);
        }

        public async Task<SpaceResponse> GetSpaceByIdAsync(Guid id)
        {
            var space = await _context.Spaces
                .Include(s => s.Location)
                .Include(s => s.Amenities)
                .FirstOrDefaultAsync(s => s.SpaceId == id);

            if (space == null) throw new NotFoundException("Space not found.");

            return MapToResponse(space);
        }

        public async Task<List<SpaceResponse>> SearchSpacesAsync(SpaceSearchRequest request)
        {
            var query = _context.Spaces
                .Include(s => s.Location)
                .Include(s => s.Amenities)
                .AsQueryable();

            // Filter by Location
            if (request.LocationId.HasValue)
            {
                query = query.Where(s => s.LocationId == request.LocationId.Value);
            }

            // Filter by Type
            if (request.Type.HasValue)
            {
                query = query.Where(s => s.Type == request.Type.Value);
            }

            // Filter by Date Availability (Basic Logic: Exclude if fully booked)
            // Note: For a robust system, this needs TimeSlot logic. 
            // Here we check if the space is not set to MAINTENANCE or RESERVED status.
            if (request.Date.HasValue)
            {
                 query = query.Where(s => s.Status == SpaceStatus.AVAILABLE);
                 
                 // Advanced: Exclude spaces that have a confirmed reservation covering the WHOLE day?
                 // Keeping it simple for browsing: showing all active spaces.
            }

            var spaces = await query.ToListAsync();
            return spaces.Select(MapToResponse).ToList();
        }

        public async Task<bool> DeleteSpaceAsync(Guid id)
        {
            var space = await _context.Spaces.FindAsync(id);
            if (space == null) return false;

            _context.Spaces.Remove(space);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<SpaceResponse> UpdateSpaceAsync(Guid id, UpdateSpaceRequest request)
        {
            var space = await _context.Spaces
                .Include(s => s.Location)
                .Include(s => s.Amenities)
                .FirstOrDefaultAsync(s => s.SpaceId == id);

            if (space == null) throw new NotFoundException("Space not found.");

            // Update fields only if they are provided (not null)
            if (!string.IsNullOrEmpty(request.Name)) space.Name = request.Name;
            if (request.Type.HasValue) space.Type = request.Type.Value;
            if (request.Capacity.HasValue) space.Capacity = request.Capacity.Value;
            if (request.PricePerHour.HasValue) space.PricePerHour = request.PricePerHour.Value;
            if (!string.IsNullOrEmpty(request.Description)) space.Description = request.Description;
            
            // Handle Status Change (Maintenance Mode logic from Sequence Diagram)
            if (request.Status.HasValue) 
            {
                space.Status = request.Status.Value;
                // NOTE: In Phase 4, you would add logic here to cancel existing reservations 
                // if status becomes MAINTENANCE.
            }

            await _context.SaveChangesAsync();
            return MapToResponse(space);
        }
        
        public async Task<List<LocationDto>> GetAllLocationsAsync()
        {
            return await _context.Locations
                .Select(l => new LocationDto 
                { 
                    LocationId = l.LocationId, 
                    Name = l.Name, 
                    City = l.City 
                })
                .ToListAsync();
        }

        private SpaceResponse MapToResponse(Space s)
        {
            return new SpaceResponse
            {
                SpaceId = s.SpaceId,
                Name = s.Name,
                Type = s.Type,
                Capacity = s.Capacity,
                PricePerHour = s.PricePerHour,
                Status = s.Status,
                LocationName = s.Location?.Name ?? "Unknown",
                Amenities = s.Amenities.Select(a => a.Name).ToList()
            };
        }
    }
}