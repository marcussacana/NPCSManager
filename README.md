## Nitro+ Console Script Manager (NPCSM) - v0.9.6
[![Build Status](https://travis-ci.org/ForumHulp/pageaddon.svg?branch=master)](http://vnx.uvnworks.com)

A DLL Library tool made to allow easy modifying of strings in C#, aimed primarily as a means to edit Nitro+/MAGES. game engine scripts.

Encoding.cs is mapped mostly for STEINS;GATE 0 (Vita) characters, but can be easily altered to work with other games and scripts.

Originally created to edit scripts from the PC version of STEINS;GATE 0, but altered to support the Vita's script files. (PC version shouldn't be too different, if at all)


## String Format

### Unknown Letter
This is a sam[0x8400]le text

[0x????] = Character missing in the Encoding.cs

When modifying Encoding.cs, remember to alter both the Decode and Encode sections to avoid problems.


### Custom colors in words
This is a <#000000[0x2414]#>sample <#FFFFFFtext

<#?????? = Beginning of the color tag

[0x????] = Unknown Tag parameter

\#> = End of color tag

Closing the tag is not necessary if the color tag is done at the end of the dialogue (not the sentence/word!).


### Unknown
This is a [0x112233...] sample text

[0x??????] \(3 bytes or more of length) = Unknown command, most likely has to do with engine triggers or flags.

Don't alter those unless you're absolutely certain of what you're doing.
