using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary;
using SharedLibrary.Requests;

namespace CatCafeGameServer.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class CatController : ControllerBase
{
    private readonly GameDbContext _dbContext;
    
    public CatController(GameDbContext dbContext)
    {
        _dbContext = dbContext;
        
        
    }
    
    [HttpGet("{id:int}")]
    public Cat Get([FromRoute] int id)
    {
        var player = new Cat() { Id = id, Level = 0};
        return player;
    }

    [HttpPost]
    public Cat Post(CreateCatRequest request)
    {
        var userId = int.Parse(User.FindFirst("id").Value);
        var user = _dbContext.Users.First(u => u.Id == userId);

        var cat = new Cat()
        {
            User = user,
            Level = 0
        };

        _dbContext.Add(cat);
        _dbContext.SaveChanges();

        return cat;
    }
}