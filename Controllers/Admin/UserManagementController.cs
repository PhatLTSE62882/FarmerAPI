using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models;
using Utility;

namespace FarmerAPI.Controllers.Admin
{
    [Authorize]
    [ApiController]
    [Route("api/admin/user")]

    public class UserManagementController : ControllerBase
    {
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;
        public UserManagementController(IUnitOfWork _unitOfWork, IMapper _mapper)
        {
            unitOfWork = _unitOfWork;
            mapper = _mapper;
        }
        [HttpGet("getall")]
        public IActionResult GetAll()
        {
            var getAll = unitOfWork.Users.GetAll();
            return Ok(getAll);
        }
        [HttpGet("getuser")]
        public IActionResult GetUser([FromQuery] String email)
        {
            var getUser = unitOfWork.Users.GetFirstOrDefault(u => u.Email == email);
            if(getUser==null)
            {
                return NotFound();
            }
            return Ok(getUser);
        }
        [HttpPost("register")]
        public IActionResult Register([FromBody] Users formUser)
        {
            formUser.LockEnabled = false;
            formUser.LockEnd = DateTime.Now;
            formUser.EmailConfirmed = false;
            formUser.Id = ConstantVariable.HashKey(formUser.Email);
            unitOfWork.Users.Add(formUser);
            unitOfWork.Save();
            return Ok("Successfully");
        }


    }
}