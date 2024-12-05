using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Bibliotec.Contexts;
using Bibliotec.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Bibliotec_mvc.Controllers
{
    [Route("[controller]")]
    public class LivroController : Controller
    {
        private readonly ILogger<LivroController> _logger;

        public LivroController(ILogger<LivroController> logger)
        {
            _logger = logger;
        }
        Context context = new Context();

        public IActionResult Index()
        {
            ViewBag.admin = HttpContext.Session.GetString("Admin");

            //Criar uma lista de livros
            List<Livro> Listalivros = context.Livro.ToList();

            // Verificar se o livro tem reserva ou não
            var livrosReservados = context.LivroReserva.ToDictionary(livro => livro.LivroID, livror => livror.DtReserva);

            ViewBag.Livros = Listalivros;
            ViewBag.LivrosComReserva = livrosReservados;

            return View();
        }
        [Route("Cadastro")]
        // Médoto que retorna a tela de Cadastro:


        public IActionResult Cadastro()
        {
            ViewBag.admin = HttpContext.Session.GetString("Admin");
            ViewBag.Categorias = context.Categoria.ToList();
            return View();
        }
        //Método para cadastrar livro:
        [Route("Cadastrar")]
        public IActionResult Cadastrar(IFormCollection form)
        {
            Livro novoLivro = new Livro();
            //O que o usuario escrever no formulario, sera atribuido no novoLivro:
            novoLivro.Nome = form["Nome"].ToString();
            novoLivro.Descricao = form["Descricao"].ToString();
            novoLivro.Editora = form["Editora"].ToString();
            novoLivro.Escritor = form["Escritor"].ToString();
            novoLivro.Idioma = form["Idioma"].ToString();
            if (form.Files.Count > 0)
            {
                var arquivo = form.Files[0];
                var pasta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/imagens/Livros");
                if (!Directory.Exists(pasta))
                {
                    Directory.CreateDirectory(pasta);
                }
                var caminho = Path.Combine(pasta, arquivo.FileName);
                using (var stream = new FileStream(caminho, FileMode.Create))
                {
                    arquivo.CopyTo(stream);
                }
                novoLivro.Imagem = arquivo.FileName;



            }
            else
            {
                novoLivro.Imagem = "padrao.png";
            }

            //IMAGEM
            context.Livro.Add(novoLivro);

            context.SaveChanges();

            List<LivroCategoria> listaLivroCategorias = new List<LivroCategoria>();
            string[] categoriasSelecionadas = form["Categoria"].ToString().Split(',');

            foreach (string categoria in categoriasSelecionadas)
            {
                LivroCategoria livroCategoria = new LivroCategoria();
                livroCategoria.CategoriaID = int.Parse(categoria);
                livroCategoria.LivroID = novoLivro.LivroID;
                listaLivroCategorias.Add(livroCategoria);

            }
            context.LivroCategoria.AddRange(listaLivroCategorias);
            context.SaveChanges();

            return LocalRedirect("/Livro/Cadastro");
        }

        [Route("Editar/{id}")]
        public IActionResult Editar(int id)
        {
            ViewBag.Admin = HttpContext.Session.GetString("Admin");
            ViewBag.categoriasDoSistema = context.Categoria.ToList();

            Livro? livroEncontrado = context.Livro.FirstOrDefault(livro => livro.LivroID == id)!;
            var categoriasDoLivroEncontrado = context.LivroCategoria.Where(identificadorLivro => identificadorLivro.LivroID == id).Select(Livro => Livro.Categoria).ToList();

            ViewBag.Livro = livroEncontrado;
            ViewBag.categoria = categoriasDoLivroEncontrado;
            return View();
        }
        [Route("Atualizar")]
        public IActionResult Atualizar(IFormCollection form, int id, IFormFile imagem)
        {
            Livro livroAtualizado = context.Livro.FirstOrDefault(livro => livro.LivroID == id)!;

            livroAtualizado.Nome = form["Nome"].ToString();
            livroAtualizado.Descricao = form["Descricao"].ToString();
            livroAtualizado.Editora = form["Editora"].ToString();
            livroAtualizado.Escritor = form["Escritor"].ToString();
            livroAtualizado.Idioma = form["Idioma"].ToString();

            if (imagem != null && imagem.Length > 0)
            {//defiinir o caminho da minha imagem do livro ATUAL, que eu quero alterar:
                var caminhoImagem = Path.Combine("wwwroot/imagens/Livros", imagem.FileName);
                if (!string.IsNullOrEmpty(livroAtualizado.Imagem))
                {
                    var caminhoImagemAntiga = Path.Combine("wwwroot/imagens/Livros", livroAtualizado.Imagem);
                    if (System.IO.File.Exists(caminhoImagemAntiga))
                    {
                        System.IO.File.Delete(caminhoImagemAntiga);
                    }
                }
                using (var stream = new FileStream(caminhoImagem, FileMode.Create))
                {
                    imagem.CopyTo(stream);
                }
                livroAtualizado.Imagem = imagem.FileName;
            }

            //PRIMEIRO: Precisamos pegar as categorias selecionadas do usuário
            var categoriasSelecionadas = form["Categoria"].ToList();
            //SEGUNDA: Pegaremos as categorias ATUAIS do livro
            var categoriasAtuais = context.LivroCategoria.Where(livro => livro.LivroID == id).ToList();
            //TERCEIRO: Removeremos as categorias antigas
            foreach (var categoria in categoriasAtuais)
            {
                if (!categoriasSelecionadas.Contains(categoria.CategoriaID.ToString()))
                {
                    context.LivroCategoria.Remove(categoria);
                }

            }
            //QUARTO: Adicionaremos as novas categorias
            foreach (var categoria in categoriasSelecionadas)
            {
                if (!categoriasAtuais.Any(c => c.CategoriaID.ToString() == categoria))
                {
                    context.LivroCategoria.Add(new LivroCategoria
                    {
                        LivroID = id,
                        CategoriaID = int.Parse(categoria)
                    });
                    context.SaveChanges();

                }

            }


            return LocalRedirect("/Livro");

            // método de excluir o livro


        }
        [Route("Excluir/{id}")]
        public IActionResult Excluir(int id)
        {
            Livro livroEncontrado = context.Livro.First(livro => livro.LivroID == id);
            var categoriasDoLivro = context.LivroCategoria.Where(livro => livro.LivroID == id).ToList();
            foreach (var categoria in categoriasDoLivro)
            {
                context.LivroCategoria.Remove(categoria);
               
            }
                context.Livro.Remove(livroEncontrado);

                context.SaveChanges();

            return LocalRedirect("/Livro");
        }

        // [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        // public IActionResult Error()
        // {
        //     return View("Error!");
        // }
    }
}
