using System.ComponentModel.DataAnnotations;
using FoodDelivery.Exception;
using FoodDelivery.Models;
using FoodDelivery.Models.Dto;
using FoodDelivery.Models.Entity;
using FoodDelivery.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodDelivery.Controllers;

[ApiController]
[Route("api/dish")]
public class DishController : ControllerBase
{

    private readonly IDishService _dishService;

    public DishController(IDishService dishService)
    {
        _dishService = dishService;
    }

    /// <summary>
    /// Получить список блюд (меню)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(void), StatusCodes.Status400BadRequest)]
    public ActionResult<DishPagesListDto> GetDishes(
        [FromQuery] List<DishCategory> categories,
        bool vegetarian = false,
        DishSorting? sorting = null,
        int page = 1)
    {
        try
        {
            return _dishService.GetDishPage(categories, vegetarian, sorting, page);
        }
        catch (PageNotFoundException)
        {
            return BadRequest(new 
            {
                Message = "Недопустимое значение для страницы атрибута"
            });
        }
    }
    
    /// <summary>
    /// Получить информацию о конкретном блюде
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    public ActionResult<DishDto> GetDishById(Guid id)
    {
        try
        {
            return _dishService.GetDishInfo(id);
        }
        catch (DishNotFoundException)
        {
            return NotFound(new 
            {
                Message = $"Блюдо с ID = {id} не в корзине"
            });
        }
    }
    
    /// <summary>
    /// Проверить, может ли пользователь установить рейтинг блюду
    /// </summary>
    [HttpGet("{id:guid}/rating/check"), Authorize]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    public ActionResult<bool> CheckCanSetRating(Guid id)
    {
        try
        {
            return _dishService.CanSetRating(User, id);
        }
        catch (DishNotFoundException)
        {
            return NotFound(new 
            {
                Message = $"Блюдо с ID = {id} не в корзине"
            });
        }
    }
    
    /// <summary>
    /// Установить рейтинг для блюда
    /// </summary>
    [HttpPost("{id:guid}/rating"), Authorize]
    [ProducesResponseType(typeof(void), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    public IActionResult SetRating(
        Guid id, 
        [Range(1, 10)] int ratingScore)
    {
        try
        {
            _dishService.SetRating(User, id, ratingScore);
            return Ok();
        }
        catch (DishNotFoundException)
        {
            return NotFound(new 
            {
                Message = $"Блюдо с ID = {id} не в корзине"
            });
        }
        catch (DishNotOrderedException)
        {
            return BadRequest(new 
            {
                Message = "Пользователь не может установить оценку блюду, которое не было заказано"
            });
        }
    }
}