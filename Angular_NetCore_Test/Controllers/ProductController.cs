using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Angular_NetCore_Test.Data;
using Angular_NetCore_Test.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Angular_NetCore_Test.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        public ProductController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext; 
        }

        // GET: api/Product
        [HttpGet("[action]")]                           
      //  [Authorize(Policy= "RequireLoggedIn")]
        public IActionResult GetProducts()
        {
            return Ok(_dbContext.Products.ToList());
        }

        [HttpPost("[action]")]        
        //[Authorize(Policy = "RequireAdministratorRole")]
        public async Task<IActionResult> AddProduct([FromBody] ProductModel formData)
        {
            var product = new ProductModel
            {
                ProductName = formData.ProductName,
                ImageUrl = formData.ImageUrl,
                Description = formData.Description,
                OutOfStock = formData.OutOfStock,
                Price = formData.Price
            };
            await _dbContext.AddAsync(product);
            await _dbContext.SaveChangesAsync();
            return Ok(new JsonResult("The product has successfully added"));
        }

        //api/product/1
        [HttpPut("[action]/{id}")]
        //[Authorize(Policy = "RequireAdministratorRole")]
        public async Task<IActionResult> UpdateProduct([FromRoute] int id, [FromBody] ProductModel formData)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); 
            }
            var findProduct = _dbContext.Products.FirstOrDefault(x => x.ProductId == id); 
            if(findProduct == null)
            {
                return NotFound(); 
            }

            findProduct.ProductName = formData.ProductName;
            findProduct.Description = formData.Description;
            findProduct.ImageUrl = formData.ImageUrl;
            findProduct.OutOfStock = formData.OutOfStock;
            findProduct.Price = formData.Price;
            _dbContext.Entry(findProduct).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();

            return Ok(new JsonResult(" The Product id "+id+"is updated"));

        }
         
        [HttpDelete("[action]/{id}")]                             
        //[Authorize(Policy = "RequireAdministratorRole")]
        public async Task<IActionResult> DeleteProduct([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); 
            }

            var findProduct = _dbContext.Products.FirstOrDefault(x => x.ProductId == id);
            if(findProduct == null)
            {
                return NotFound(); 
            }
            _dbContext.Products.Remove(findProduct);
            await _dbContext.SaveChangesAsync();

            return Ok(new JsonResult("The product containing id " + id + " is deltede ")); 
        }

    }
}
