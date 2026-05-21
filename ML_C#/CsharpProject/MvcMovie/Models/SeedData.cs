using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MvcMovie.Data;
using System;
using System.Linq;

namespace MvcMovie.Models;

public static class SeedData
{
    public static void Initialize(IServiceProvider serviceProvider)
    {
        using (var context = new MvcMovieContext(
            serviceProvider.GetRequiredService<
                DbContextOptions<MvcMovieContext>>()))
        {
            // Look for any movies.
            if (context.Movie.Any())
            {
                return;   // DB has been seeded
            }
            context.Movie.AddRange(
                new Movie
                {
                    Title = "When Harry Met Sally",
                    ReleaseDate = DateTime.Parse("1989-2-12"),
                    Genre = "Romantic Comedy",
                    Rating = "R",
                    Price = 7.99M
                },
                new Movie
                {
                    Title = "Ghostbusters ",
                    ReleaseDate = DateTime.Parse("1984-3-13"),
                    Genre = "Comedy",
                    Rating = "PG",
                    Price = 8.99M
                },
                new Movie
                {
                    Title = "Ghostbusters 2",
                    ReleaseDate = DateTime.Parse("1986-2-23"),
                    Genre = "Comedy",
                    Rating = "PG",
                    Price = 9.99M
                },
                new Movie
                {
                    Title = "Rio Bravo",
                    ReleaseDate = DateTime.Parse("1959-4-15"),
                    Genre = "Western",
                    Rating = "NR",
                    Price = 3.99M
                },
                new Movie
                {
                    Title = "Spider-Man: Across the Spider-Verse",
                    ReleaseDate = DateTime.Parse("2023-6-2"),
                    Genre = "Animation",
                    Rating = "PG",
                    Price = 14.99M
                },
                new Movie
                {
                    Title = "The Batman",
                    ReleaseDate = DateTime.Parse("2022-3-4"),
                    Genre = "Action",
                    Rating = "PG-13",
                    Price = 12.99M
                },
                new Movie
                {
                    Title = "The Lord of the Rings: The Fellowship of the Ring",
                    ReleaseDate = DateTime.Parse("2001-12-19"),
                    Genre = "Fantasy",
                    Rating = "PG-13",
                    Price = 10.99M
                },
                new Movie
                {
                    Title = "The Lord of the Rings: The Two Towers",
                    ReleaseDate = DateTime.Parse("2002-12-18"),
                    Genre = "Fantasy",
                    Rating = "PG-13",
                    Price = 10.99M
                },
                new Movie
                {
                    Title = "The Core",
                    ReleaseDate = DateTime.Parse("2003-3-28"),
                    Genre = "Sci-Fi",
                    Rating = "PG-13",
                    Price = 7.99M
                }
            );
            context.SaveChanges();
        }
    }
}