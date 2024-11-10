using AutoMapper;
using FilmesAPI.Data;
using FilmesAPI.Data.Dtos;
using FilmesAPI.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace FilmesAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class FilmeController : ControllerBase
{
    private FilmeContext _context;
    private IMapper _mapper;

    public FilmeController(FilmeContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    /// <summary>
    /// Adiciona um filme ao banco de dados
    /// </summary>
    /// <param name="filmeDto">Objeto com os campos necessários para criação de um filme</param>
    /// <returns>IActionResult</returns>
    /// <response code="201">Caso inserção seja feita com sucesso</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public IActionResult AdicionaFilme([FromBody] CreateFilmeDto filmeDto)
    {
        Filme filme = _mapper.Map<Filme>(filmeDto);
        _context.Filmes.Add(filme);
        _context.SaveChanges();
        return CreatedAtAction(nameof(RecuperaFilmesPorId),
            new { id = filme.Id },
            filme);
    }

    /// <summary>
    /// Recupera a lista de filmes com base nos filtros aplicados
    /// </summary>
    /// <param name="skip">Número de registros a pular para paginação</param>
    /// <param name="take">Número de registros a retornar para paginação</param>
    /// <returns>Uma lista de filmes</returns>
    /// <response code="200">Retorna a lista de filmes</response>
    /// <response code="404">Se não houver filmes para retornar</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IEnumerable<ReadFilmeDto> RecuperaFilmes([FromQuery] int skip = 0,[FromQuery] int take = 50)
    { 
        return _mapper.Map<List<ReadFilmeDto>>(_context.Filmes.Skip(skip).Take(take)); 
    }

    /// <summary>
    /// Recupera um filme específico pelo ID
    /// </summary>
    /// <param name="id">ID do filme a ser recuperado</param>
    /// <returns>O filme com o ID especificado</returns>
    /// <response code="200">Retorna o filme solicitado</response>
    /// <response code="404">Se o filme não for encontrado</response>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult RecuperaFilmesPorId(int id)
    {
        var filme = _context.Filmes.FirstOrDefault(filme => filme.Id == id);
        if (filme == null) return NotFound();

        var filmeDto = _mapper.Map<ReadFilmeDto>(filme);
        return Ok(filmeDto);
    }

    /// <summary>
    /// Atualiza um filme existente
    /// </summary>
    /// <param name="id">ID do filme a ser atualizado</param>
    /// <param name="filmeDto">Objeto com os dados atualizados do filme</param>
    /// <returns>IActionResult</returns>
    /// <response code="204">Caso a atualização seja bem-sucedida</response>
    /// <response code="404">Caso o filme não seja encontrado</response>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult UpdateFilme(int id, [FromBody] UpdateFilmeDto filmeDto)
    {
        var filme = _context.Filmes.FirstOrDefault(filme => filme.Id == id);
        if (filme == null) return NotFound();
        _mapper.Map(filmeDto, filme);
        _context.SaveChanges();
        return NoContent();
    }

    /// <summary>
    /// Atualiza parcialmente os dados de um filme
    /// </summary>
    /// <param name="id">ID do filme a ser atualizado</param>
    /// <param name="patch">Operação de patch contendo as mudanças</param>
    /// <returns>IActionResult</returns>
    /// <response code="204">Caso a atualização seja bem-sucedida</response>
    /// <response code="404">Caso o filme não seja encontrado</response>
    /// <response code="400">Caso a requisição esteja mal-formada</response>
    [HttpPatch("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult AtualizaFilmeParcial(int id, JsonPatchDocument<UpdateFilmeDto> patch)
    {
        var filme = _context.Filmes.FirstOrDefault(filme => filme.Id == id);
        if (filme == null) return NotFound();

        var filmeParaAtualizar = _mapper.Map<UpdateFilmeDto>(filme);
        patch.ApplyTo(filmeParaAtualizar, ModelState);

        if (!TryValidateModel(filmeParaAtualizar))
            return ValidationProblem(ModelState);

        _mapper.Map(filmeParaAtualizar, filme);
        _context.SaveChanges();
        return NoContent();
    }

    /// <summary>
    /// Remove um filme pelo ID
    /// </summary>
    /// <param name="id">ID do filme a ser deletado</param>
    /// <returns>IActionResult</returns>
    /// <response code="204">Caso o filme seja deletado com sucesso</response>
    /// <response code="404">Caso o filme não seja encontrado</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult DeletaFilme(int id)
    {
        var filme = _context.Filmes.FirstOrDefault(filme => filme.Id == id);
        if (filme == null)
        {
            return NotFound();
        }
        _context.Remove(filme);
        _context.SaveChanges();
        return NoContent();

    }
}
