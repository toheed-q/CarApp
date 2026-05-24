using AutoMapper;
using DMF_Services.Data;
using DMF_Services.DTOs.UserDetails;
using DMF_Services.Models;
using DMF_Services.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DMF_Services.Services
{
    public class UserDetailService : IUserDetailService
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;

        public UserDetailService(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<IEnumerable<UserDetailDto>> GetAllAsync()
        {
            var entities = await _db.UserDetails.AsNoTracking().ToListAsync();
            return _mapper.Map<IEnumerable<UserDetailDto>>(entities);
        }

        public async Task<UserDetailDto?> GetByIdAsync(int id)
        {
            var entity = await _db.UserDetails.FindAsync(id);
            return entity == null ? null : _mapper.Map<UserDetailDto>(entity);
        }

        public async Task<UserDetailDto?> GetByMobileNoAsync(string mobile, bool isActive = true)
        {
            var entity = await _db.UserDetails.FirstOrDefaultAsync(ud => ud.PrimaryMobile == mobile && ud.IsActive == isActive);
            return entity == null ? null : _mapper.Map<UserDetailDto>(entity);
        }

        public async Task<(UserDetailDto UserDetail, bool IsCreated)> CreateAsync(CreateUserDetailDto dto)
        {
            var existingUser = await _db.UserDetails
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.PrimaryMobile == dto.PrimaryMobile);

            if (existingUser != null)
            {
                return (_mapper.Map<UserDetailDto>(existingUser), false); // Not created
            }


            var entity = _mapper.Map<UserDetail>(dto);
            _db.UserDetails.Add(entity);
            await _db.SaveChangesAsync();

            return (_mapper.Map<UserDetailDto>(entity), true); // Newly created
        }

        public async Task UpdateAsync(int id, UpdateUserDetailDto dto)
        {
            var entity = await _db.UserDetails.FindAsync(id);
            if (entity == null) throw new KeyNotFoundException();

            _mapper.Map(dto, entity);
            await _db.SaveChangesAsync();
        }
    }
}
