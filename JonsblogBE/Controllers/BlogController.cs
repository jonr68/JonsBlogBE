using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JonsblogBE.Data;
using JonsblogBE.Models;

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
    
    [HttpGet("files")]
    public IActionResult GetFileList()
    {
        var dir = Path.Combine(Directory.GetCurrentDirectory(), "BlogFiles");
        var files = Directory.GetFiles(dir)
                             .Select(Path.GetFileName)
                             .ToArray();
        return Ok(files);
    }

    [HttpGet("files/{fileName}")]
    public IActionResult GetHtmlFile(string fileName)
    {
        var safeFileName = Path.GetFileName(fileName);
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "BlogFiles", safeFileName);

        if (!System.IO.File.Exists(filePath))
        {
            return NotFound();
        }

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
        var fileName = await WriteBlogFileAsync(blog.Title, blog.Content);

        return Ok(new { fileName });
    }

    private static string BuildBlogHtml(string title, string content)
    {
        var encodedTitle = WebUtility.HtmlEncode(title);
        var encodedContent = WebUtility.HtmlEncode(content).Replace("\n", "<br>");
        const string stylesheetUrl = "http://localhost:5000/styles/generated.css";

        return $$"""
                 <!DOCTYPE html>
                 <html lang="en">
                 <head>
                     <meta charset="UTF-8">
                     <title>{{encodedTitle}}</title>
                     <link href="{{stylesheetUrl}}" rel="stylesheet">
                 </head>
                 <body class="h-screen bg-blue-700">
                 <div>
                     <nav>
                         <ul class="flex justify-center space-x-4 p-4 bg-gradient-to-tr from-amber-700 via-amber-400 to-yellow-200 rounded-lg text-amber-950 font-bold">
                             <li><a href="#" class="font-bold text-black hover:text-gray-500">Home</a></li>
                             <li><a href="#" class="font-bold text-black hover:text-gray-500">About</a></li>
                             <li><a href="#" class="font-bold text-black hover:text-gray-500">Blog</a></li>
                         </ul>
                     </nav>
                 </div>
                 <div class="flex justify-center px-4 py-8">
                     <div class="relative p-6 bg-[#2a2a2a] rounded-[40px] border-[12px] border-[#1a1a1a] shadow-[0_20px_50px_rgba(0,0,0,0.5),inset_0_2px_5px_rgba(255,255,255,0.1)]">
                         <div class="rounded-[20px] overflow-hidden shadow-[inset_0_5px_15px_rgba(0,0,0,0.8),0_1px_1px_rgba(255,255,255,0.1)]">
                             <div class="bg-zinc-950 p-10 min-h-[300px] relative">
                                 <div class="w-full max-w-3xl bg-black shadow-sm border border-slate-200 rounded-lg p-10">
                                     <h1 class="font-mono text-green-500 [text-shadow:0_0_5px_rgba(34,197,94,0.8)] text-4xl mb-6 text-center">
                                         {{encodedTitle}}
                                     </h1>
                                     <p class="text-center font-mono text-green-500 [text-shadow:0_0_5px_rgba(34,197,94,0.8)]">
                                         {{encodedContent}}
                                     </p>
                                 </div>
                             </div>
                         </div>
                     </div>
                 </div>
                 </body>
                 </html>
                 """;
    }

    private static string Slugify(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "blog";
        }

        var invalidChars = Path.GetInvalidFileNameChars();
        var cleaned = new string(value.Trim().Select(ch => invalidChars.Contains(ch) ? '-' : ch).ToArray());
        cleaned = cleaned.Replace(' ', '-');

        return string.IsNullOrWhiteSpace(cleaned) ? "blog" : cleaned;
    }
    
    [HttpPost]
    public async Task<ActionResult<Blog>> Create(Blog blog)
    {
        _context.BlogPosts.Add(blog);
        await _context.SaveChangesAsync();
        await WriteBlogFileAsync(blog.Title, blog.Content);
        return CreatedAtAction(nameof(Get), new { id = blog.Id }, blog);
    }

    private static async Task<string> WriteBlogFileAsync(string title, string content)
    {
        var safeTitle = Slugify(title);
        var datePart = DateTime.Now.ToString("MM-dd-yyyy");
        var fileName = $"{safeTitle}-{datePart}.html";
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "BlogFiles", fileName);

        var html = BuildBlogHtml(title, content);
        await System.IO.File.WriteAllTextAsync(filePath, html);

        return fileName;
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