using System.Net;
using FluentAssertions;
using Kontur.BigLibrary.Service.Contracts;
using Kontur.BigLibrary.Tests.Core.ApiClients;
using Kontur.BigLibrary.Tests.Core.Helpers;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Tests.ApiSystem;

public class BooksApiTests : BooksApiTestBase
{
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        authApiClient = new AuthApiClient();
        booksApiClient = new BooksApiClient();
    }

    [Test]
    public void AddNewBook_Correct_Success()
    {
        //Arrange
        var user = CreateUser();
        var imageId = booksApiClient.CreateImage(ValidImagePath, user.token).Content;
        var book = new BookBuilder().WithImage(int.Parse(imageId)).Build();
        
        //Act
        var response = booksApiClient.AddBookToLibrary(book, user.token);
        
        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Should().NotBeNull();
        var savedBook = JsonConvert.DeserializeObject<Book>(response.Content!);
        savedBook.Should().BeEquivalentTo(book);
    }

    [Test]
    public void Enqueue_ShouldBeSuccessful_WhenBookIsNotTaken()
    {
        var userName = "Aboba Abobovich";
        var user = CreateUser();
        var book = CreateBook(user.token);
        
        var response = booksApiClient.EnqueueBook(book.Id.ToString()!, userName, user.token);
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Should().NotBeNull();
        response.Content.Should().Contain("Книга свободна.");

        var queue = booksApiClient.GetReadersInQueueDeserialized(book.Id.ToString()!, user.token);
        queue.Should().BeEmpty();
    }
    
    [Test]
    public void Enqueue_ShouldBeSuccessful_WhenBookIsTaken()
    {
        var userName = "Aboba TheFirst";
        var userName2 = "Aboba TheSecond";
        var user = CreateUser();
        var user2 = CreateUser();
        var book = CreateBook(user.token);
        
        var response = booksApiClient.CheckoutBook(book.Id.ToString()!, userName, user.token);
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Should().NotBeNull();
        response.Content.Should().Contain("Вы взяли книгу.");
        
        var response2 = booksApiClient.EnqueueBook(book.Id.ToString()!, userName2, user2.token);
        
        response2.StatusCode.Should().Be(HttpStatusCode.OK);
        response2.Content.Should().NotBeNull();
        response2.Content.Should().Contain("Вы встали в очередь.");
        
        var queue = booksApiClient.GetReadersInQueueDeserialized(book.Id.ToString()!, user.token);
        queue.Should().ContainSingle().Which.UserName.Should().Be(userName2);
    }
    
    [Test]
    public void Enqueue_ShouldBeSuccessful_WhenBookIsTakenByTheSameUser()
    {
        var userName = "Aboba Aboba";
        var user = CreateUser();
        var book = CreateBook(user.token);
        
        var response = booksApiClient.CheckoutBook(book.Id.ToString()!, userName, user.token);
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Should().NotBeNull();
        response.Content.Should().Contain("Вы взяли книгу.");
        
        var response2 = booksApiClient.EnqueueBook(book.Id.ToString()!, userName, user.token);
        
        response2.StatusCode.Should().Be(HttpStatusCode.OK);
        response2.Content.Should().NotBeNull();
        response2.Content.Should().Contain("Вы уже взяли эту книгу.");
        
        var queue = booksApiClient.GetReadersInQueueDeserialized(book.Id.ToString()!, user.token);
        queue.Should().BeEmpty();
    }
    
    [Test]
    public void Enqueue_ShouldBeSuccessful_WhenUserIsAlreadyInQueue()
    {
        var userName = "Aboba TheThird";
        var userName2 = "Aboba TheFourth";
        var user = CreateUser();
        var user2 = CreateUser();
        var book = CreateBook(user.token);
        
        var response = booksApiClient.CheckoutBook(book.Id.ToString()!, userName, user.token);
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Should().NotBeNull();
        response.Content.Should().Contain("Вы взяли книгу.");
        
        var response2 = booksApiClient.EnqueueBook(book.Id.ToString()!, userName2, user2.token);
        
        response2.StatusCode.Should().Be(HttpStatusCode.OK);
        response2.Content.Should().NotBeNull();
        response2.Content.Should().Contain("Вы встали в очередь.");
        
        var response3 = booksApiClient.EnqueueBook(book.Id.ToString()!, userName2, user2.token);
        
        response3.StatusCode.Should().Be(HttpStatusCode.OK);
        response3.Content.Should().NotBeNull();
        response3.Content.Should().Contain("Вы уже стоите в очереди.");
        
        var queue = booksApiClient.GetReadersInQueueDeserialized(book.Id.ToString()!, user.token);
        queue.Should().ContainSingle().Which.UserName.Should().Be(userName2);
    }
    
    [Test]
    public void CheckoutBook_ShouldBeSuccessful_WhenBookIsNotTaken()
    {
        var userName = "Abobus Abobov";
        var user = CreateUser();
        var book = CreateBook(user.token);
        
        var response = booksApiClient.CheckoutBook(book.Id.ToString()!, userName, user.token);
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Should().NotBeNull();
        response.Content.Should().Contain("Вы взяли книгу.");

        var readers = booksApiClient.GetBookReadersDeserialized(book.Id.ToString()!, user.token);
        readers.Should().ContainSingle().Which.UserName.Should().Be(userName);
    }
    
    [Test]
    public void CheckoutBook_ShouldBeSuccessful_WhenBookIsTaken()
    {
        var userName = "Abobus TheFirst";
        var userName2 = "Abobus TheSecond";
        var user = CreateUser();
        var user2 = CreateUser();
        var book = CreateBook(user.token);
        
        var response = booksApiClient.CheckoutBook(book.Id.ToString()!, userName, user.token);
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Should().NotBeNull();
        response.Content.Should().Contain("Вы взяли книгу.");
        
        var response2 = booksApiClient.CheckoutBook(book.Id.ToString()!, userName2, user2.token);
        
        response2.StatusCode.Should().Be(HttpStatusCode.OK);
        response2.Content.Should().NotBeNull();
        response2.Content.Should().Contain("Книга занята.");
        
        var readers = booksApiClient.GetBookReadersDeserialized(book.Id.ToString()!, user.token);
        readers.Should().ContainSingle().Which.UserName.Should().Be(userName);
    }
    
    [Test]
    public void CheckoutBook_ShouldBeSuccessful_WhenBookIsFreeButUserNotFirstInQueue()
    {
        var userName = "Abobus one";
        var userName2 = "Abobus two";
        var userName3 = "Abobus three";
        var user = CreateUser();
        var user2 = CreateUser();
        var user3 = CreateUser();
        var book = CreateBook(user.token);
        
        var takeResponse = booksApiClient.CheckoutBook(book.Id.ToString()!, userName3, user3.token);
        
        takeResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        takeResponse.Content.Should().NotBeNull();
        takeResponse.Content.Should().Contain("Вы взяли книгу.");
        
        var enqueueResponse = booksApiClient.EnqueueBook(book.Id.ToString()!, userName, user.token);
        
        enqueueResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        enqueueResponse.Content.Should().NotBeNull();
        enqueueResponse.Content.Should().Contain("Вы встали в очередь.");
        
        var freeResponse = booksApiClient.ReturnBook(book.Id.ToString()!, userName3, user3.token);
        
        freeResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        freeResponse.Content.Should().NotBeNull();
        freeResponse.Content.Should().Contain("Книга свободна.");
        
        var response = booksApiClient.CheckoutBook(book.Id.ToString()!, userName2, user2.token);
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Should().NotBeNull();
        response.Content.Should().Contain("Не ваша очередь.");
        
        var readers = booksApiClient.GetBookReadersDeserialized(book.Id.ToString()!, user.token);
        readers.Should().BeEmpty();
        
        var queue = booksApiClient.GetReadersInQueueDeserialized(book.Id.ToString()!, user.token);
        queue.Should().ContainSingle().Which.UserName.Should().Be(userName);
    }
    
    [Test]
    public void CheckoutBook_ShouldBeSuccessful_WhenBookIsFreeAndUserIsFirstInQueue()
    {
        var userName = "Abobus given";
        var userName2 = "Abobus taken";
        var user = CreateUser();
        var user2 = CreateUser();
        var book = CreateBook(user.token);
        
        var takeResponse = booksApiClient.CheckoutBook(book.Id.ToString()!, userName, user.token);
        
        takeResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        takeResponse.Content.Should().NotBeNull();
        takeResponse.Content.Should().Contain("Вы взяли книгу.");
        
        var enqueueResponse = booksApiClient.EnqueueBook(book.Id.ToString()!, userName2, user2.token);
        
        enqueueResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        enqueueResponse.Content.Should().NotBeNull();
        enqueueResponse.Content.Should().Contain("Вы встали в очередь.");
        
        var freeResponse = booksApiClient.ReturnBook(book.Id.ToString()!, userName, user.token);
        
        freeResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        freeResponse.Content.Should().NotBeNull();
        freeResponse.Content.Should().Contain("Книга свободна.");
        
        var response = booksApiClient.CheckoutBook(book.Id.ToString()!, userName2, user2.token);
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Should().NotBeNull();
        response.Content.Should().Contain("Вы взяли книгу.");
        
        var readers = booksApiClient.GetBookReadersDeserialized(book.Id.ToString()!, user.token);
        readers.Should().ContainSingle().Which.UserName.Should().Be(userName2);
    }
}