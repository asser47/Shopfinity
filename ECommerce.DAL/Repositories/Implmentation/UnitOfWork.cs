using ECommerce.DAL.Data;
using ECommerce.DAL.Models;
using ECommerce.DAL.Repositories.Implmentation;
using ECommerce.DAL.Repositories.Interfaces;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    public IRepository<Product> Products { get; private set; }
    public IRepository<Category> Categories { get; private set; }  // ✅ Just generic!
    public IRepository<Cart> Carts { get; private set; }
    public IRepository<CartItem> CartItems { get; private set; }
    public IRepository<Order> Orders { get; private set; }
    public IRepository<OrderItem> OrderItems { get; private set; }

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;

        Products = new Repository<Product>(_context);
        Categories = new Repository<Category>(_context);  // ✅ Just generic!
        Carts = new Repository<Cart>(_context);
        CartItems = new Repository<CartItem>(_context);
        Orders = new Repository<Order>(_context);
        OrderItems = new Repository<OrderItem>(_context);
    }

    public int Complete()
    {
        return _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}