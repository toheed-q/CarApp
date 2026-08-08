using AutoMapper;
using DMF_Services.Data;
using DMF_Services.DTOs.DealerRequests;
using DMF_Services.Models;
using DMF_Services.Services.Interfaces;

namespace DMF_Services.Services
{
    public class DealerRequestService : IDealerRequestService
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;

        public DealerRequestService(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<DealerRequestDto> CreateAsync(CreateDealerRequestDto dto)
        {
            var entity = _mapper.Map<DealerRequest>(dto);
            entity.Status = "Pending";
            entity.CreatedDate = DateTime.UtcNow;

            _db.DealerRequests.Add(entity);
            await _db.SaveChangesAsync();

            return _mapper.Map<DealerRequestDto>(entity);
        }
    }
}
