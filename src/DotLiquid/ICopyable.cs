using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotLiquid
{

    /// <summary>
    /// An alternative to ICloneable which is not recommended for use by Microsoft:
    /// "Do not implement ICloneable. There are two general ways to implement ICloneable,
    /// either as a deep, or non-deep copy. Deep-copy copies the cloned object and all
    /// objects referenced by the object, recursively until all objects in the graph are
    /// copied. A non-deep copy (referred to as ‘shallow’ if only the top level references
    /// are copied) may do none, or part of a deep copy. Because the interface contract
    /// does not specify the type of clone performed, different classes have different
    /// implementations. A consumer cannot rely on ICloneable to let them know whether
    /// an object is deep-cloned or not."
    /// This interface indicates that the object can be copied for the purpose of mutability
    /// during render and is primarily used when changes need to be made to the node tree
    /// during the render phase to create clones of rendered objects so that the original
    /// remains safe and immutable.
    /// </summary>
    public interface ICopyable
    {
        object Copy();
    }
}
