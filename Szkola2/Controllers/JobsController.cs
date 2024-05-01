using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Szkola2.Data;
using Szkola2.Models;

namespace Szkola2.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class JobsController : ControllerBase
	{
        public class DataTransferJob
        {
            private readonly ApplicationDbContext _dbContext;

            public DataTransferJob(ApplicationDbContext dbContext)
            {
                _dbContext = dbContext;
            }

            public async Task CopyAndArchiveTextsAsync()
            {
                var endDate = new DateTime(DateTime.Now.Year, 8, 31);
                var startDate = new DateTime(DateTime.Now.Year - 1, 8, 31);

                var textsFromLastYear = await _dbContext.Texts
                    .Where(t => t.DateAdded >= startDate && t.DateAdded <= endDate && (t.CategoryId == 1 || t.CategoryId == 2))
                    .ToListAsync();

                foreach (var text in textsFromLastYear)
                {
                    var pastSchoolEvent = new PastSchoolEvent
                    {
                        EventName = text.TextName,
                        EventDate = text.EventDate,
                        Content = text.Content,
                        MainGraphic = text.MainGraphic,
                        GraphicGallery = text.GraphicGallery,
                    };
                    _dbContext.PastSchoolEvents.Add(pastSchoolEvent);
                }

                var recentTexts = await _dbContext.Texts
                    .Where(t => t.IsActive == true)
                    .OrderByDescending(t => t.DateAdded)
                    .Take(50)
                    .ToListAsync();
                var allTexts = await _dbContext.Texts.ToListAsync();
                foreach (var text in allTexts)
                {
                    text.IsActive = recentTexts.Contains(text);
                }

                await _dbContext.SaveChangesAsync();
            }
        }
    }
}
