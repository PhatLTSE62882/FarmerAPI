using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FarmerAPI.Controllers.Admin
{
    [Route("api/admin/frequency")]
    [ApiController]
    [Authorize]
    public class FrequencyManagementController : ControllerBase
    {
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;
        public FrequencyManagementController(IUnitOfWork _unitOfWork, IMapper _mapper)
        {
            unitOfWork = _unitOfWork;
            mapper = _mapper;
        }
        [HttpGet("getall")]
        public IActionResult GetAll()
        {
            var getAll = unitOfWork.Frequency.GetAll();
            return Ok(getAll);
        }
        [HttpGet("detail")]

        public IActionResult GetDetail([FromQuery] int id)
        {
            var getDetails = unitOfWork.Frequency.GetFirstOrDefault(d => d.Id == id);
            if (getDetails == null)
            {
                return NotFound();
            }
            return Ok(getDetails);
        }
        [HttpPost("update")]

        public IActionResult Update([FromBody] Models.Frequency frequency)
        {
            if (frequency.Id == 0)
            {
                unitOfWork.Frequency.Add(frequency);
            }
            else
            {
                var getDetails = unitOfWork.Frequency.GetFirstOrDefault(d => d.Id == frequency.Id);
                if (getDetails == null)
                {
                    return Ok(false);
                }
                unitOfWork.Frequency.Update(frequency);
            }
            unitOfWork.Save();
            return Ok(true);
        }
        [HttpGet("render")]
        public IActionResult RenderHTML()
        {
            // read excel

            List<String> excelColumns = new List<string>();
            excelColumns.Add("FirstName");
            excelColumns.Add("LastName");
            excelColumns.Add("Address");
            excelColumns.Add("Phone");
            excelColumns.Add("Email");
            var renderHTML = "";
            foreach (var item in excelColumns)
            {
                renderHTML += "<th>" + item + "</th>";
            }
            return Ok(renderHTML);
        }

    }
}