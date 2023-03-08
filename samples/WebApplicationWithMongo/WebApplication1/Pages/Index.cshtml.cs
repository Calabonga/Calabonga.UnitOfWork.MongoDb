
using Calabonga.UnitOfWork.MongoDb;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Driver;
using WebApplicationWithMongo.Application;
using WebApplicationWithMongo.Pages.Orders;

namespace WebApplicationWithMongo.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(IUnitOfWork unitOfWork, ILogger<IndexModel> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public IPagedList<Order>? Data { get; set; }

        public async Task<IActionResult> OnGetAsync(int pageIndex = 1, int pageSize = 10)
        {
            _logger.LogInformation("Checking replica ready");
            _unitOfWork.EnsureReplicationSetReady();

            var repository = _unitOfWork.GetRepository<Order, int>();
            Data = await repository.GetPagedAsync(pageIndex - 1, pageSize,
                FilterDefinition<Order>.Empty,
                Builders<Order>.Sort.Ascending(x => x.Number), HttpContext.RequestAborted);

            return Page();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            var repository = _unitOfWork.GetRepository<Order, int>();

            var item = OrderHelper.GetRandomOrder();

            try
            {
                var session = await _unitOfWork.GetSessionAsync(HttpContext.RequestAborted);
                await repository.Collection.InsertOneAsync(session, item, null, HttpContext.RequestAborted);
                TempData["Message"] = $"success:New order successfully added with title {item.Title} and SKU #{item.Id}";
            }
            catch (Exception exception)
            {
                TempData["Message"] = $"danger:Error from Insert [{exception.GetBaseException().Message}]";
            }
            return RedirectToPage("Index");
        }

        public async Task<IActionResult> OnGetDeleteAsync(int id)
        {
            var repository = _unitOfWork.GetRepository<Order, int>();

            try
            {
                var session = await _unitOfWork.GetSessionAsync(HttpContext.RequestAborted);
                var result = await repository.Collection.DeleteOneAsync(session, Builders<Order>.Filter.Eq(x => x.Id, id),
                    null, HttpContext.RequestAborted);
                TempData["Message"] = $"info:New order successfully deleted (IsAcknowledged {result.IsAcknowledged}) (DeletedCount {result.DeletedCount})";
            }
            catch (Exception exception)
            {
                TempData["Message"] = $"danger:Error when Delete [{exception.GetBaseException().Message}]";
            }
            return RedirectToPage("Index");
        }
    }
}