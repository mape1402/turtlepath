namespace TurtlePath.EntityFrameworkCore.Conventions
{
    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// Applies a TurtlePath convention to an EF Core model.
    /// </summary>
    public interface ITurtlePathModelConvention
    {
        /// <summary>
        /// Applies the convention to the specified model builder.
        /// </summary>
        /// <param name="builder">The EF Core model builder.</param>
        /// <param name="options">The TurtlePath DbContext options.</param>
        void Apply(ModelBuilder builder, TurtlePathDbContextOptions options);
    }
}
