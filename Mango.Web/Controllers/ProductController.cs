using Mango.Web.Models;
using Mango.Web.Models.Dto;
using Mango.Web.Service.IService;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Reflection;

namespace Mango.Web.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        public async Task<IActionResult> ProductIndex()
        {
            List<ProductDTO>? list = new();

            ResponseDto? response = await _productService.GetAllProductAsync();

            if (response != null && response.IsSuccess)
            {
                list.AddRange(JsonConvert.DeserializeObject<List<ProductDTO>>(Convert.ToString(response.Result)));
            }
            else
            {
                TempData["error"] = response?.Message;
            }

            return View(list);
        }

        public async Task<IActionResult> ProductCreate()
        {
            return View();
        }

        [HttpGet]
		public async Task<IActionResult> ProductEdit(int Id)
		{
            ResponseDto response = await _productService.GetProductByIdAsync(Id);

            if (response!=null && response.IsSuccess)
            {
                ProductDTO productDto = JsonConvert.DeserializeObject<ProductDTO>(Convert.ToString(response.Result));
                return View(productDto);
            }
            else
            {
                TempData["error"] = "Unable to Edit";
                return RedirectToAction(nameof(ProductIndex));
            }
		}

		[HttpPost]
		public async Task<IActionResult> ProductEdit(ProductDTO model)
		{
			if (ModelState.IsValid)
			{
				ResponseDto? response = await _productService.UpdateProductAsync(model);

				if (response != null && response.IsSuccess)
				{
					TempData["success"] = "Product Updated Successfully";
					return RedirectToAction(nameof(ProductIndex));
				}
				else
				{
					TempData["error"] = response?.Message;
				}
			}

			return View(model);
		}

		[HttpPost]
		public async Task<IActionResult> ProductCreate(ProductDTO model)
		{
            if (ModelState.IsValid)
            {
				ResponseDto? response = await _productService.CreateProductAsync(model);

				if (response != null && response.IsSuccess)
				{
                    TempData["success"] = "Product Created Successfully";
                    return RedirectToAction(nameof(ProductIndex));
				}
                else
                {
                    TempData["error"] = response?.Message;
                }
            }

			return View(model);
		}

		public async Task<IActionResult> ProductDelete(int Id)
		{
			ResponseDto? response = await _productService.GetProductByIdAsync(Id);

			if (response != null && response.IsSuccess)
			{
				ProductDTO? model = JsonConvert.DeserializeObject<ProductDTO>(Convert.ToString(response.Result));
				return View(model);
			}
            else
            {
                TempData["error"] = response?.Message;
			}

            return NotFound();
		}

        [HttpPost]
        public async Task<IActionResult> ProductDelete(ProductDTO model)
        {
            ResponseDto? response = await _productService.DeleteProductAsync(model.ProductId);

            if (response != null && response.IsSuccess)
            {
                TempData["success"] = "Product Deleted Successfully";
                return RedirectToAction(nameof(ProductIndex));
            }
            else
			{
				TempData["error"] = response?.Message;

				response = await _productService.GetProductByIdAsync(model.ProductId);

				if (response != null && response.IsSuccess)
				{
					ProductDTO? model1 = JsonConvert.DeserializeObject<ProductDTO>(Convert.ToString(response.Result));
					return View(model1);
				}
            }

            return View(model);
        }
    }
}
