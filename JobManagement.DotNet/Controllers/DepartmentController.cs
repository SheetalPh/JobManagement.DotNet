﻿using JobManagement.DotNet.Data;
using JobManagement.DotNet.Modals.Entities;
using JobManagement.DotNet.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace JobManagement.DotNet.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")] ///controller fo managing Departments
    public class DepartmentController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;

        public DepartmentController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        ////Department related methods 

        [HttpGet]
        public async Task<IActionResult> GetallDepartments()
        {
            var departments = await dbContext.Departments.ToListAsync();
            return Ok(departments);
        }

        [HttpGet("{id}")] // Fetch particular Department details by the Department id 
        public async Task<IActionResult> GetDepartmentById(int id)
        {
            try
            {
                var departments = await dbContext.Departments.FirstOrDefaultAsync(j => j.Id == id);

                if (departments == null)
                {
                    return NotFound(); // Job with the specified ID was not found
                }

                return Ok(departments); // Return the found job
            }

            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request");
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddDepartment(AddDepartmentDto addDepartmentDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Create a new Department entity with the provided title
            var departmentEntity = new Department
            {
                Title = addDepartmentDto.Title
                // No need to set the ID as it's autogenerated by the database
            };

            dbContext.Departments.Add(departmentEntity);
            await dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetDepartmentById), new { id = departmentEntity.Id }, departmentEntity);
        }

        [HttpPut("{id}")] //update a exisiting department feilds  by passing the department id 

        public async Task<IActionResult> UpdateDepartment(int id, UpdateDepartmentDto updateDepartmentDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var Departments = await dbContext.Departments.FindAsync(id);
            if (Departments == null)
            {
                return NotFound(); // Job with the specified ID was not found
            }

            Departments.Title = updateDepartmentDto.Title;


            dbContext.Entry(Departments).State = EntityState.Modified;
            await dbContext.SaveChangesAsync();

            return Ok(); // Return the updated job entity with a 200 OK status code
        }

    }
}
