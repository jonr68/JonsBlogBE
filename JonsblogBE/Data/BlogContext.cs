using JonsblogBE.Models;
using Microsoft.EntityFrameworkCore;

namespace JonsblogBE.Data;

public class BlogContext : DbContext
{
    public BlogContext(DbContextOptions<BlogContext> options) : base(options)
    {
    }

    public DbSet<Blog> BlogPosts { get; set; }
}


