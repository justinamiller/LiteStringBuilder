# LiteStringBuilder

Alternative to the System.Text.StringBuilder C# class.

Why ? Because System.Text.StringBuilder does actually a lot of memory allocation when appending string, very often it's just not better than a direct string concat.

