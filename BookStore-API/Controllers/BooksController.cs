using AutoMapper;
using BookStore_API.Contracts;
using BookStore_API.Data;
using BookStore_API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookStore_API.Controllers
{
    /// <summary>
    /// Endpoint used to interact with the Books in the book store's database
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BooksController : ControllerBase
    {
        private readonly IBookRepository _bookRepository;
        private readonly IMapper _mapper;
        private readonly ILoggerService _logger;

        public BooksController(IBookRepository bookRepository, IMapper mapper, ILoggerService logger)
        {
            _bookRepository = bookRepository;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Get all Books
        /// </summary>
        /// <returns>List of Books</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBooks()
        {
            var location = GetControllerActionNames();

            try
            {
                _logger.LogInfo($"{location}: Attempted call.");

                var books = await _bookRepository.FindAll();
                var response = _mapper.Map<IList<BookDTO>>(books);

                _logger.LogInfo($"{location}: Successful.");

                return Ok(response);
            }
            catch (Exception ex)
            {
                return InternalError($"{location}: Error", ex);
            }
        }

        /// <summary>
        /// Get a Book by Id
        /// </summary>
        /// <param name="id">Book Id</param>
        /// <returns>A Books's record</returns>
        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBook(int id)
        {
            var location = GetControllerActionNames();

            try
            {
                _logger.LogInfo($"{location}: Attempted call for id: {id}");
                                
                if (! await _bookRepository.IsExists(id))
                {
                    var warnMessage = $"{location}: Id: {id} was not found.";

                    _logger.LogWarn(warnMessage);
                    return NotFound(warnMessage);
                }

                var book = await _bookRepository.FindById(id);
                var response = _mapper.Map<BookDTO>(book);

                _logger.LogInfo($"{location}: Successfully got {response.Title}");

                return Ok(response);
            }
            catch (Exception ex)
            {
                return InternalError($"{location}: Error getting id: {id}", ex);
            }
        }

        /// <summary>
        /// Creates a Book
        /// </summary>
        /// <param name="bookDTO"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] BookCreateDTO bookDTO)
        {
            var location = GetControllerActionNames();

            try
            {
                _logger.LogInfo($"{location}: Submission attempted.");

                if (bookDTO == null)
                {
                    _logger.LogInfo($"{location}: Empty request was submitted.");
                    return BadRequest(ModelState);
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogWarn($"{location}: Data was incomplete.");
                    return BadRequest(ModelState);
                }

                var book = _mapper.Map<Book>(bookDTO);

                var isSuccess = await _bookRepository.Create(book);

                if (!isSuccess)
                {                    
                    return InternalError($"{location}: Creation failed.");
                }

                return Created("Create", new { book });
            }
            catch (Exception ex)
            {
                return InternalError($"{location}: Error", ex);
            }
        }

        /// <summary>
        /// Updates an Book
        /// </summary>
        /// <param name="id">Book Id</param>
        /// <param name="bookDTO"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int id, [FromBody] BookUpdateDTO bookDTO)
        {
            var location = GetControllerActionNames();

            try
            {
                _logger.LogInfo($"{location}: Update attempted for id: {id}");

                if (id < 1 || bookDTO == null || id != bookDTO.Id)
                {
                    _logger.LogInfo($"{location}: Update failed with bad data.");
                    return BadRequest(ModelState);
                }

                if (!await _bookRepository.IsExists(id))
                {
                    var warnMessage = $"{location}: Id: {id} was not found.";

                    _logger.LogWarn(warnMessage);
                    return NotFound(warnMessage);
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogWarn($"{location}: Data was incomplete.");
                    return BadRequest(ModelState);
                }

                var book = _mapper.Map<Book>(bookDTO);

                var isSuccess = await _bookRepository.Update(book);

                if (!isSuccess)
                {                    
                    return InternalError($"{location}: Update failed.");
                }

                _logger.LogInfo($"{location}: Id: {id} successfully updated.");
                return NoContent();
            }
            catch (Exception ex)
            {
                return InternalError($"{location}: Error", ex);
            }
        }

        /// <summary>
        /// Removes a Book by id
        /// </summary>
        /// <param name="id">Book Id</param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id)
        {
            var location = GetControllerActionNames();

            try
            {
                _logger.LogInfo($"{location}: Delete attempted for id: {id}");

                if (id < 1)
                {
                    _logger.LogWarn($"{location}: Delete failed with bad data");
                    return BadRequest();
                }

                if (!await _bookRepository.IsExists(id))
                {
                    var warnMessage = $"{location}: Id: {id} was not found.";

                    _logger.LogWarn(warnMessage);
                    return NotFound(warnMessage);
                }

                var book = await _bookRepository.FindById(id);
                var isSuccess = await _bookRepository.Delete(book);

                if (!isSuccess)
                {                    
                    return InternalError($"{location}: Delete failed.");
                }

                _logger.LogInfo($"{location}: Id: {id} successfully deleted.");
                return NoContent();
            }
            catch (Exception ex)
            {
                return InternalError($"{location}: Error", ex);
            }
        }

        private string GetControllerActionNames()
        {
            var controller = ControllerContext.ActionDescriptor.ControllerName;
            var action = ControllerContext.ActionDescriptor.ActionName;

            return $"{controller} - {action}";
        }

        private ObjectResult InternalError(string message, Exception ex = null)
        {
            if(ex != null)
            {
                _logger.Log(message, ex);
                return StatusCode(500, ex.Message);
            }
           else
            {
                _logger.LogError(message);
                return StatusCode(500, message);
            }
        }
    }
}
