using ComaxRpUI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.Text.Json;
using AutoMapper.QueryableExtensions;

namespace GrainStorageService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StateController : ControllerBase
    {
        private readonly RpDbContext _storageDbContext;
        private readonly AutoMapper.IMapper _mapper;
        private readonly ILogger<StateController> _logger;
        public StateController(RpDbContext storageDbContext, AutoMapper.IMapper mapper, ILogger<StateController> logger)
        {
            _storageDbContext = storageDbContext;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet("List")]
        public async Task<IActionResult> List(int page, int itemsPerPage = 50)
        {
            try
            {
                var coll = _storageDbContext.Set<RpEntry>();
                var qry = coll.Skip((page - 1) * itemsPerPage).Take(itemsPerPage);
                var res = (await qry.ToListAsync()).ConvertAll(x => _mapper.Map<RpEntryVm>(x));
                return Ok(res);
            }
            catch (Exception ex)
            {
                _logger.LogError("Could not fetch list", ex);
                throw;
            }
        }

        [HttpPut("{id}")]
        [HttpPost("{id}")]
        public async Task<IActionResult> Upsert(Guid id, [FromBody] RpEntryVm data)
        {
            if(!this.ModelState.IsValid)
            {
                return ValidationProblem(this.ModelState);
            }
            try
            {
                var coll = _storageDbContext.Set<RpEntry>();

                var current = await coll.FindAsync(id);
                if(current == null)
                {
                    data.Id = id;
                    var newVal = _mapper.Map<RpEntry>(data);
                    coll.Add(newVal);
                }
                else
                {
                    _mapper.Map(data, current);
                }
                
                await _storageDbContext.SaveChangesAsync();

                return Ok();

            }
            catch (Exception ex)
            {
                return this.UnprocessableEntity();
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            
            try
            {
                var coll = _storageDbContext.Set<RpEntry>();
                var res = await coll.FindAsync(id);
                if (res == null)
                    return NotFound();

                coll.Remove(res);

                await _storageDbContext.SaveChangesAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                return this.UnprocessableEntity();
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {

            try
            {
                var coll = _storageDbContext.Set<RpEntry>();
                var res = await coll.FindAsync(id);
                if (res == null)
                    return NotFound();
                var retVal = _mapper.Map<RpEntryVm>(res);
                return Ok(retVal);
            }
            catch (Exception ex)
            {
                return this.UnprocessableEntity();
            }
        }
    }
}
