using Microsoft.AspNetCore.Mvc;

namespace BookMVC.Controllers
{
    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public int Year { get; set; }
        public bool IsAvailable { get; set; } = true;
    }

    public interface IBookRepository
    {
        IEnumerable<Book> GetAll();
        Book? GetById(int id);
        void Add(Book book);
        void Update(Book book);
        void Delete(int id);
    }

    [ApiController]
    [Route("api/[controller]")]
    public class BooksController : ControllerBase
    {
        private readonly IBookRepository _repo;

        public BooksController(IBookRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public ActionResult<IEnumerable<Book>> GetAll()
        {
            var books = _repo.GetAll();
            return Ok(books);
        }

        [HttpGet("{id:int}")]
        public ActionResult<Book> GetById(int id)
        {
            var book = _repo.GetById(id);
            if (book == null) return NotFound();
            return Ok(book);
        }

        [HttpPost]
        public IActionResult Create([FromBody] Book book)
        {
            if (book == null || string.IsNullOrWhiteSpace(book.Title))
                return BadRequest("Title is required");

            _repo.Add(book);
            return CreatedAtAction(nameof(GetById), new { id = book.Id }, book);
        }

        [HttpPut("{id:int}")]
        public IActionResult Update(int id, [FromBody] Book book)
        {
            if (book == null)
                return BadRequest();

            var existing = _repo.GetById(id);
            if (existing == null) return NotFound();

            book.Id = id;
            _repo.Update(book);
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            var existing = _repo.GetById(id);
            if (existing == null) return NotFound();

            _repo.Delete(id);
            return NoContent();
        }
    }
}
