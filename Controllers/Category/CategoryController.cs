using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using DataAccess.Repository.IRepository;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.AutoMapper;
using Models.ViewModels;
using Utility;

namespace FarmerAPI.Controllers.Category
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;
        public CategoryController(IUnitOfWork _unitOfWork, IMapper _mapper)
        {
            unitOfWork = _unitOfWork;
            mapper = _mapper;
        }
        [HttpGet("GetAll")]
        public IActionResult GetCategoryForDropDown()
        {
            var getAllCategory = unitOfWork.Category.GetAll();
            //var categoryList = mapper.Map<IEnumerable<CategoryMapper>>(getAllCategory);
            var getAllService = unitOfWork.Service.GetAll(includeProperties: "Frequency");
            var HomeVM = new HomeViewModel()
            {
                CategoryList = mapper.Map<IEnumerable<CategoryMapper>>(getAllCategory),
                ServiceList = mapper.Map<IEnumerable<ServiceMapper>>(getAllService)

            };

            //var getFirst = mapper.Map<CategoryMapper>(unitOfWork.Category.GetFirstOrDefault());
            //var getFirst = mapper.Map<CategoryMapper>(category);

            return Ok(HomeVM);
        }
        [HttpPost("summary")]
        public IActionResult SummaryPOST([FromBody] OrderHeaderViewModel form)
        {
            var orderHeader = new OrderHeader
            {
                Name = form.Name,
                Status=ConstantVariable.StatusSubmitted,
                Address = form.Address,
                City=form.City,
                Email=form.Email,
                OrderDate=DateTime.Now,
                Phone=form.Phone,
                ServiceCount=form.serviceItems.Count,
                ZipCode=form.ZipCode
            };
            unitOfWork.OrderHeader.Add(orderHeader);
            unitOfWork.Save();
            foreach (var item in form.serviceItems)
            {
                var details = new OrderDetails
                {
                    OrderHeaderId = orderHeader.Id,
                    Price = item.Price,
                    ServiceId = item.Id,
                    ServiceName = item.Name
                };
                unitOfWork.OrderDetails.Add(details);
            }
            unitOfWork.Save();
            return Ok("Ordered Successfully");
        }
    }
}