using AutoMapper;
using BookStore_API.Contracts;
using BookStore_API.Data;
using BookStore_API.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookStore_API.Controllers
{
    /// <summary>
    /// Endpoint used to interact with the Authors in the book store's database
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]   
    public class AuthorsController : ControllerBase
    {
        private readonly IAuthorRepository _authorRepository;
        private readonly IMapper _mapper;
        private readonly ILoggerService _logger;

        public AuthorsController(IAuthorRepository authorRepository, IMapper mapper, ILoggerService logger)
        {
            _authorRepository = authorRepository;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Get all Authors
        /// </summary>
        /// <returns>List of Authors</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAuthors()
        {
            try
            {
                _logger.LogInfo("Attempted Get all Authors.");
                
                var authors = await _authorRepository.FindAll();
                var response = _mapper.Map<IList<AuthorDTO>>(authors);
                
                _logger.LogInfo("Successfully Got all Authors.");

                return Ok(response);
            }
            catch (Exception ex)
            {
                return InternalError("Error getting all Authors", ex);
            }            
        }

        /// <summary>
        /// Get an Author by Id
        /// </summary>
        /// <param name="id">Author Id</param>
        /// <returns>An Author's record</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAuthor(int id)
        {
            try
            {
                _logger.LogInfo($"Attempted Get Author with id: {id}");
                
                var author = await _authorRepository.FindById(id);
                if (author == null)
                {
                    var warnMessage = $"Author with id: {id} was not found.";

                    _logger.LogWarn(warnMessage);
                    return NotFound(warnMessage);
                }                
                
                var response = _mapper.Map<AuthorDTO>(author);
                
                _logger.LogInfo($"Successfully Got Author {response.Firstname} {response.Lastname}");

                return Ok(response);
            }
            catch (Exception ex)
            {
                return InternalError($"Error getting Author with id: {id}", ex);
            }
        }

        /// <summary>
        /// Creates an Author
        /// </summary>
        /// <param name="authorDTO"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] AuthorCreateDTO authorDTO)
        {
            try
            {
                _logger.LogInfo($"Author submission attempted.");

                if (authorDTO == null)
                {
                    _logger.LogInfo("Empty request was submitted.");
                    return BadRequest(ModelState);
                }

                if(!ModelState.IsValid)
                {
                    _logger.LogWarn("Author data was incomplete.");
                    return BadRequest(ModelState);
                }

                var author = _mapper.Map<Author>(authorDTO);

                var isSuccess = await _authorRepository.Create(author);

                if(!isSuccess)
                {
                    _logger.LogWarn("Author creation failed.");
                    return StatusCode(500, "Author creation failed.");
                }

                return Created("Create", new { author });
            }
            catch (Exception ex)
            {
                return InternalError($"Error creating Author.", ex);
            }
        }

        /// <summary>
        /// Creates an Author
        /// </summary>
        /// <param name="id">Author Id</param>
        /// <param name="authorDTO"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int id, [FromBody] AuthorUpdateDTO authorDTO)
        {
            try
            {
                _logger.LogInfo($"Author with id: {id} update attempted.");

                if (id < 1 || authorDTO == null || id != authorDTO.Id)
                {
                    _logger.LogInfo("Author update failed with bad data.");
                    return BadRequest(ModelState);
                }

                if (!await _authorRepository.IsExists(id))
                {
                    var warnMessage = $"Author with id: {id} was not found.";

                    _logger.LogWarn(warnMessage);
                    return NotFound(warnMessage);
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogWarn("Author data was incomplete.");
                    return BadRequest(ModelState);
                }

                var author = _mapper.Map<Author>(authorDTO);

                var isSuccess = await _authorRepository.Update(author);

                if (!isSuccess)
                {
                    _logger.LogWarn("Author update failed.");
                    return StatusCode(500, "Author update failed.");
                }

                _logger.LogInfo($"Author with id: {id} successfully updated.");
                return NoContent();
            }
            catch (Exception ex)
            {
                return InternalError($"Error updating Author.", ex);
            }
        }

        /// <summary>
        /// Removes an Author by id
        /// </summary>
        /// <param name="id">Author Id</param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                _logger.LogInfo($"Author with id: {id} delete attempted.");

                if (id < 1)
                {
                    _logger.LogWarn("Author delete failed with bad data");
                    return BadRequest();
                }

                if (!await _authorRepository.IsExists(id))
                {
                    var warnMessage = $"Author with id: {id} was not found.";

                    _logger.LogWarn(warnMessage);
                    return NotFound(warnMessage);
                }

                var author = await _authorRepository.FindById(id);
                var isSuccess = await _authorRepository.Delete(author);

                if (!isSuccess)
                {
                    _logger.LogWarn("Author delete failed.");
                    return StatusCode(500, "Author delete failed.");
                }

                _logger.LogInfo($"Author with id: {id} successfully deleted.");
                return NoContent();
            }
            catch (Exception ex)
            {
                return InternalError($"Error deleting Author.", ex);
            }
        }

        private ObjectResult InternalError(string message, Exception ex)
        {
            _logger.Log(message, ex);
            return StatusCode(500, ex.Message);
        }
    }
}
