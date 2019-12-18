using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Utility;
using Models;

namespace FarmerAPI.Controllers.Admin
{
    [Route("api/admin/category")]
    [ApiController]
    [Authorize]
    public class CategoryManagementController : ControllerBase
    {
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;
        public CategoryManagementController(IUnitOfWork _unitOfWork, IMapper _mapper)
        {
            unitOfWork = _unitOfWork;
            mapper = _mapper;
        }
        [HttpGet("getall")]
        
        public IActionResult GetAll()
        {
            var getAll = unitOfWork.Category.GetAll();
            return Ok(getAll);
        }
        [HttpGet("detail")]

        public IActionResult GetDetail([FromQuery] int id)
        {
            var getDetails = unitOfWork.Category.GetFirstOrDefault(d=>d.Id==id);
            if(getDetails == null)
            {
                return NotFound();
            }
            return Ok(getDetails);
        }
        [HttpPost("update")]

        public IActionResult Update([FromBody] Models.Category category)
        {
            if (category.Id == 0)
            {
                unitOfWork.Category.Add(category);
            }
            else
            {
            var getDetails = unitOfWork.Category.GetFirstOrDefault(d => d.Id == category.Id);

            if (getDetails == null)
            {
                return Ok(false);
            }     
                unitOfWork.Category.Update(category);
            }
            unitOfWork.Save();
            return Ok(true);
        }


    }
}