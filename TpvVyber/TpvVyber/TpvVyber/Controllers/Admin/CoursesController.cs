using Microsoft.AspNetCore.Mvc;
using TpvVyber.Classes;
using TpvVyber.Client.Classes;
using TpvVyber.Data;

namespace TpvVyber.Controllers;

[ApiController]
[Route("api/courses")]
public class CoursesController : ControllerBase
{
    private async Task<CourseCln> addIntern(CourseCln item, TpvVyberContext context)
    {
        var newEntity = Course.ToServer(item, context, createNew: true);
        var element = await context.Courses.AddAsync(newEntity);
        await context.SaveChangesAsync();

        return context.Courses.Find(element.Entity)?.ToClient(context) ?? throw new Exception(
                "Nepodařilo se přidat do databáze"
            );
    }

    private async Task deleteIntern(CourseCln item, TpvVyberContext context)
    {
        var entityToDelete = Course.ToServer(item, context);
        context.Courses.Remove(entityToDelete);
        await context.SaveChangesAsync();
    }

    private async Task updateIntern(CourseCln item, TpvVyberContext context)
    {
        var entityToUpdate = Course.ToServer(item, context);
        context.Courses.Update(entityToUpdate);
        await context.SaveChangesAsync();
    }

    [HttpGet("get_all")]
    public ActionResult<IEnumerable<CourseCln>> GetAll([FromServices] TpvVyberContext context)
    {
        try
        {
            return Ok(context.Courses.Select(course => course.ToClient(context)).ToList());
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("get_by_id")]
    public ActionResult<CourseCln?> GetById(int id, [FromServices] TpvVyberContext context)
    {
        try
        {
            return Ok(context.Courses.Find(id)?.ToClient(context));
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("add")]
    public async Task<ActionResult<CourseCln>> Add(
        [FromBody] CourseCln item,
        [FromServices] TpvVyberContext context
    )
    {
        try
        {
            return Ok(await addIntern(item, context));
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("delete")]
    public async Task<IActionResult> Delete(
        [FromBody] CourseCln item,
        [FromServices] TpvVyberContext context
    )
    {
        try
        {
            await deleteIntern(item, context);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("update")]
    public async Task<IActionResult> Update(
        [FromBody] CourseCln item,
        [FromServices] TpvVyberContext context
    )
    {
        try
        {
            await updateIntern(item, context);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
