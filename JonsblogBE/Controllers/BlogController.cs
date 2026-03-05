
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JonsblogBE.Data;
using JonsblogBE.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace JonsblogBE.Controllers;

[ApiController]
[Route("blogs")]
public class BlogController : ControllerBase
{
    private readonly BlogContext _context;

    public BlogController(BlogContext context)
    {
        _context = context;
    }
    
    [HttpGet("files/{fileName}")]
    public IActionResult GetHtmlFile(string fileName)
    {
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "BlogFiles", fileName);

        return PhysicalFile(filePath, "text/html");
    }
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Blog>>> GetAll()
    {
        return await _context.BlogPosts.ToListAsync();
    }
    
    [HttpPost("files")]
    public async Task<IActionResult> SavePost([FromBody] Blog blog)
    {
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "BlogFiles", blog.Title);
        await System.IO.File.WriteAllTextAsync(filePath, blog.Content);
        return Ok();
    }
    
    [HttpPost]
    public async Task<ActionResult<Blog>> Create(Blog blog)
    {
        _context.BlogPosts.Add(blog);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = blog.Id }, blog);
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<Blog>> Get(int id)
    {
        var post = await _context.BlogPosts.FindAsync(id);
        return post == null ? NotFound() : post;
    }
    
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Blog blog)
    {
        if (id != blog.Id) return BadRequest("Id doesn't match");
        _context.Entry(blog).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return NoContent();
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var post = await _context.BlogPosts.FindAsync(id);
        if (post == null) return NotFound();
        _context.BlogPosts.Remove(post);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}