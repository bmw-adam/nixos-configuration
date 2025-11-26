using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TpvVyber.Data;

namespace TpvVyber.Controllers;

public class BaseAdminController<T> : ControllerBase
    where T : class
{
    private async Task<T> addIntern(T item, TpvVyberContext context)
    {
        var element = context.Set<T>().Add(item);
        await context.SaveChangesAsync();
        return context.Set<T>().Find(element.Entity) ?? throw new Exception(
                "Nepodařilo se přidat do databáze"
            );
    }

    private async Task deleteIntern(T item, TpvVyberContext context)
    {
        context.Set<T>().Remove(item);
        await context.SaveChangesAsync();
    }

    private async Task updateIntern(T item, TpvVyberContext context)
    {
        context.Set<T>().Attach(item);
        context.Entry(item).State = EntityState.Modified;
        await context.SaveChangesAsync();
    }

    [HttpGet("get_all")]
    public ActionResult<IEnumerable<T>> GetAll([FromServices] TpvVyberContext context)
    {
        try
        {
            return Ok(context.Set<T>().ToList());
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("get_by_id")]
    public ActionResult<T> GetById(int id, [FromServices] TpvVyberContext context)
    {
        try
        {
            return Ok(context.Set<T>().Find());
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("add")]
    public async Task<ActionResult<T>> Add(
        [FromBody] T item,
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
        [FromBody] T item,
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
        [FromBody] T item,
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
