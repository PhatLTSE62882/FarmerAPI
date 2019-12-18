using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.ViewModels;
using Newtonsoft.Json;

namespace FarmerAPI.Controllers.Admin
{
    [Route("api/admin/service")]
    [ApiController]
    [Authorize]
    public class ServiceManagementController : ControllerBase
    {
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;
        public ServiceManagementController(IUnitOfWork _unitOfWork, IMapper _mapper)
        {
            unitOfWork = _unitOfWork;
            mapper = _mapper;
        }
        [HttpGet("getall")]
        public IActionResult GetAll()
        {
            var getAll = unitOfWork.Service.GetAll(includeProperties: "Frequency,Category");
            return Ok(getAll);
        }
        [HttpDelete("delete")]
        public IActionResult Delete([FromQuery] int id)
        {
            var getService = unitOfWork.Service.GetFirstOrDefault(x => x.Id == id);
            if(getService == null)
            {
                return Ok(false);
            }
            unitOfWork.Service.Remove(getService);
            unitOfWork.Save();
            return Ok(true);
        }
        [HttpGet("detail")]
        public IActionResult GetDetail([FromQuery] int id)
        {
            var serviceDetail = new ServiceDetailViewModel();
            if(id==0)
            {
               // var getDetails = unitOfWork.Service.GetFirstOrDefault();
                var getAllFrequency = unitOfWork.Frequency.GetAll();
                var getAllCategory = unitOfWork.Category.GetAll();
                serviceDetail.Service = new Service();
                serviceDetail.CategoryList = getAllCategory;
                serviceDetail.FrequencyList = getAllFrequency;
            }
            else
            {
                var getDetails = unitOfWork.Service.GetFirstOrDefault();
                var getAllFrequency = unitOfWork.Frequency.GetAll();
                var getAllCategory = unitOfWork.Category.GetAll();
                serviceDetail.Service = getDetails;
                serviceDetail.CategoryList = getAllCategory;
                serviceDetail.FrequencyList = getAllFrequency;
            }
            
            return Ok(serviceDetail);
        }
      
        [HttpPost("update")]
        public async Task<IActionResult> UpdateOrInsert([FromForm] IFormFile fileToUpload,[FromForm] String service)
        {
            // var files = Request.Form.Files[0];
            var getService = JsonConvert.DeserializeObject<Service>(service);
            if(getService.Id==0)
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/services", fileToUpload.FileName);
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await fileToUpload.CopyToAsync(stream);
                }
                getService.ImageUrl = fileToUpload.FileName;
                unitOfWork.Service.Add(getService);
            }else
            {
                var serviceObj = unitOfWork.Service.Get(getService.Id);
                if (fileToUpload.FileName.Length>0)
                {
                    var oldImage = serviceObj.ImageUrl;
                    if (oldImage != null)
                    {
                        var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/services", oldImage);
                        if (System.IO.File.Exists(path))
                        {
                            System.IO.File.Delete(path);
                        }
                         path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/services", fileToUpload.FileName);

                        using (var stream = new FileStream(path, FileMode.Create))
                        {
                            await fileToUpload.CopyToAsync(stream);
                        }
                        getService.ImageUrl = fileToUpload.FileName;
                    }
                    else
                    {
                        var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/services", fileToUpload.FileName);

                        using (var stream = new FileStream(path, FileMode.Create))
                        {
                            await fileToUpload.CopyToAsync(stream);
                        }
                        getService.ImageUrl = fileToUpload.FileName;
                    }
                }
                unitOfWork.Service.Update(getService);
            }
            unitOfWork.Save();
            return Ok(true);
         }
    }
}