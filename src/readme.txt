Changes from Liquid Ruby:

1. Changed tag construction: instead of the constructor being passed the tagName,
   markup and tokens, these parameters are sent to a separate Initialize method,
   which is called immediately after the constructor. This prevents problems with
   virtual methods such as Parse being called in the base Block class.