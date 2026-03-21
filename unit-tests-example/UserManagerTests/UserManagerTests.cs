using NSubstitute;
using NUnit.Framework;
using UserCreatorTask;
using UserCreatorTask.UserValidators;

namespace UserManagerTests;

[Parallelizable(ParallelScope.All)]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class UserManagerTests
{
    public UserManagerTests()
    {
        _user1 = new User("Aboba Abobovich", "123456", "aboba@gmai.com", 12);
        _user2 = new User("Kto-to", "rererererer", "ktoto@mail.ru", 32);
        _user3 = new User("1", "2", "numbers@gmail.com", 18);
        _newValidUser = new User("Valid name", "valid password", "valid email address", 24);
        _newInvalidUser = new User("Invalid name", "invalid password", "invalid email address", 421);
        _allSavedUsers = [_user1, _user2, _user3];
    }

    [SetUp]
    public void SetUp()
    {
        _userRepository = Substitute.For<IUsersRepository>();
        _emailService = Substitute.For<IEmailService>();
        _userValidator = Substitute.For<IUserValidator>();
        _userRepository.GetAllUsers().Returns(_allSavedUsers);

        foreach (var user in _allSavedUsers)
        {
            _userRepository.GetUser(user.Email).Returns(user);
            _userValidator.Validate(user).Returns((true, ""));
        }

        _userValidator.Validate(_newInvalidUser).Returns((false, "Invalid user"));
        _userValidator.Validate(_newValidUser).Returns((true, ""));

        _userManager = new UserManager(_userRepository, _emailService, _userValidator);
    }

    [Test]
    public void CreateUser_ShouldSaveUserAndSandEmail_IfNewUserIsValid()
    {
        _userManager.CreateNewUser(_newValidUser);

        Received.InOrder(() =>
        {
            _userValidator.Validate(_newValidUser);
            _userRepository.GetUser(_newValidUser.Email);
            _userRepository.SaveUser(_newValidUser);
            _emailService.SendEmail(_newValidUser.Name, "Welcome", "Thank you for registering!");
        });
    }

    [Test]
    public void CreateUser_ShouldThrowInvalidUserExceptionWithReason_IfNewUserValidationFailed()
    {
        Assert.That(() => _userManager.CreateNewUser(_newInvalidUser),
            Throws.InstanceOf<InvalidUserException>().With.Message.EqualTo("Invalid user"));
        
        _userValidator.Received(1).Validate(_newInvalidUser);
        _emailService.DidNotReceive().SendEmail(_newInvalidUser.Name, "Welcome", "Thank you for registering!");
        _userRepository.DidNotReceive().SaveUser(_newInvalidUser);
        _userRepository.DidNotReceive().GetUser(_newInvalidUser.Email);
    }

    [Test]
    public void CreateUser_ShouldThrowInvalidUserExceptionWithReason_IfUserWithThatEmailAlreadyExists()
    {
        Assert.That(() => _userManager.CreateNewUser(_user1),
            Throws.InstanceOf<InvalidUserException>().With.Message.EqualTo("UserAlreadyExists"));

        Received.InOrder(() =>
        {
            _userValidator.Validate(_user1);
            _userRepository.GetUser(_user1.Email);
        });

        _emailService.DidNotReceive().SendEmail(_user1.Name, "Welcome", "Thank you for registering!");
        _userRepository.DidNotReceive().SaveUser(_user1);
    }

    [Test]
    public void DeleteUser_ShouldThrowInvalidOperationException_IfUserDoesNotExists()
    {
        Assert.That(() => _userManager.DeleteUser(_newInvalidUser.Email),
            Throws.InvalidOperationException.With.Message.EqualTo("User not found"));

        _userRepository.Received(1).GetUser(_newInvalidUser.Email);
        _emailService.DidNotReceive().SendEmail(_newInvalidUser.Name, "Goodbye", "Your account has been deleted.");
        _userRepository.DidNotReceive().DeleteUser(_newInvalidUser.Email);
    }

    [Test]
    public void DeleteUser_ShouldDeleteUserAndSendEmail_IfUserExists()
    {
        _userManager.DeleteUser(_user1.Email);

        Received.InOrder(() =>
        {
            _userRepository.GetUser(_user1.Email);
            _userRepository.DeleteUser(_user1.Email);
            _emailService.SendEmail(_user1.Name, "Goodbye", "Your account has been deleted.");
        });
    }

    [Test]
    public void GetAdultUsers_ShouldReturnUsersWithAgeGreaterOrEqualToEighteen()
    {
        var result = _userManager.GetAdultUsers();

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.EquivalentTo(new List<User> { _user2, _user3 }));
        Assert.That(result.Select(x => x.Age), Is.All.GreaterThanOrEqualTo(18));

        _userRepository.Received(1).GetAllUsers();
    }

    private IUsersRepository _userRepository;
    private IEmailService _emailService;
    private IUserValidator _userValidator;
    private UserManager _userManager;
    private readonly User _user1;
    private readonly User _user2;
    private readonly User _user3;
    private readonly User _newInvalidUser;
    private readonly User _newValidUser;
    private readonly List<User> _allSavedUsers;
}