using JobManagement.DotNet.Data;
using JobManagement.DotNet.Modals.Entities;
using JobManagement.DotNet.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Http.Logging;
using System.Linq;
using System.Threading.Tasks;

namespace JobManagement.DotNet.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")] ///controller fo managing jobs
    public class JobsController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;

        public JobsController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        //Jobs related methods 
        [HttpGet]
        public async Task<IActionResult> GetallJobs() // getalljobs returns a list of jobs 

        {
            try
            {
                var jobs = await dbContext.Jobs
                .Include(j => j.Location)
                .Include(j => j.Department)
                .Select(j => new
                {
                    j.Id,
                    j.Code,
                    j.Title,
                    j.Description,
                    j.Location,
                    j.Department,
                    j.PostedDate,
                    j.ClosingDate
                })
                .ToListAsync();

                return Ok(jobs);
            }
            catch (Exception ex)
            {

                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request");
            }

        }

        [HttpPost]
        public async Task<IActionResult> AddJob(AddjobDto addjobDto) // AddJob is used to add a new job in the system 
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingJobs = await dbContext.Jobs.Include(j => j.Location)
                .Include(j => j.Department)
                .Select(j => new
                {
                    j.Id,
                    j.Code,
                    j.Title,
                    j.Description,
                    j.Location,
                    j.Department,
                    j.PostedDate,
                    j.ClosingDate
                })
                .ToListAsync();
            string prefix = "JOB-";

            // Extract numeric parts and find the maximum value
            int maxNumber = existingJobs
                .Where(j => j.Code.StartsWith(prefix))
                .Select(j => int.TryParse(j.Code.Substring(prefix.Length), out int num) ? num : 0)
                .DefaultIfEmpty(0)
                .Max();

            // Increment the max number to generate the next job code
            int nextNumber = maxNumber + 1;
            string nextJobCode = prefix + nextNumber.ToString("D2");

            var jobEntity = new Job
            {
                Code = nextJobCode,
                Title = addjobDto.Title,
                Description = addjobDto.Description,
                LocationId = addjobDto.LocationId,
                DepartmentId = addjobDto.DepartmentId,
                ClosingDate = addjobDto.ClosingDate
                // Assuming PostedDate is set to the current date/time
            };

            dbContext.Jobs.Add(jobEntity);
            await dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetJobById), new { id = jobEntity.Id }, jobEntity);
        }

        [HttpPut("{id}")] //update a exisiting job feilds  by passing the job id and the required chnages 
       public async Task<IActionResult> UpdateJob(int id, UpdateJobDto updateJobDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var job = await dbContext.Jobs.FindAsync(id);
            if (job == null)
            {
                return NotFound(); // Job with the specified ID was not found
            }

            job.Title = updateJobDto.Title;
            job.Description = updateJobDto.Description;
            job.LocationId = updateJobDto.LocationId;
            job.DepartmentId = updateJobDto.DepartmentId;
            job.ClosingDate = updateJobDto.ClosingDate;

            dbContext.Entry(job).State = EntityState.Modified;
            await dbContext.SaveChangesAsync();

            return Ok(); // Return the updated job entity with a 200 OK status code
        }

        [HttpGet("{id}")] // Fetch particular Job details by the Job id 
        public async Task<IActionResult> GetJobById(int id)
        {
            try
            {
                var job = await dbContext.Jobs
                .Include(j => j.Location)
                .Include(j => j.Department)
                 .Select(j => new
                 {
                     j.Id,
                     j.Code,
                     j.Title,
                     j.Description,
                     j.Location,
                     j.Department,
                     j.PostedDate,
                     j.ClosingDate
                 })
                .FirstOrDefaultAsync(j => j.Id == id);

                if (job == null)
                {
                    return NotFound(); // Job with the specified ID was not found
                }

                return Ok(job); // Return the found job
            }

            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request");
            }
        }

        [HttpPost("List")] // returns a list of jobs that exists in the system pagination,search,search by dept & Location Id logic is Incorperated here 
        public async Task<IActionResult> GetallList(JobListRequest request)
        {
            try
            {
                var query = dbContext.Jobs
            .Include(j => j.Location)
            .Include(j => j.Department)
            .Select(j => new
            {
                j.Id,
                j.Code,
                j.Title,
                j.Description,
                j.Location,
                j.Department,
                j.PostedDate,
                j.ClosingDate
            })
            .AsQueryable();


                // Apply filters based on request parameters
                if (!string.IsNullOrWhiteSpace(request.Q))
                {
                    query = query.Where(j =>

                        j.Title.Contains(request.Q)
                    );
                }
                if (request.LocationId.HasValue)
                {
                    query = query.Where(j => j.Location.Id == request.LocationId);
                }
                if (request.DepartmentId.HasValue)
                {
                    query = query.Where(j => j.Department.Id == request.DepartmentId);
                }


                // Get total count before pagination
                var total = await query.CountAsync();

                // Apply pagination
                var data = await query
                    .Skip((request.PageNo - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToListAsync();

                return Ok(new { total, data });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request");
            }
        }
        //end 



    }


}
