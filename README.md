## Nitro+ Console Script Manager (NPCSM) - v0.9 - Need Char Remap
[![Build Status](https://travis-ci.org/ForumHulp/pageaddon.svg?branch=master)](http://vnx.uvnworks.com)

A DLL Library tool to allow you write your own string editor in C#

The Encoding.cs contains a big map for S;G 0 Letters, but i see wrongs mapping, have too much letters and i don't have plan to translate this game, if you need you can remap manually in the Encoding.cs, just give a value between 0x8000~0x9D00 and test in game...

Created to edit scripts from Steins;Gate 0: PC

If You like, give-me a star plz~~


##String Format

###Unknown Letter
This is a sam[0x8400]le text

[0x????] = Letter missing in the Encoding.cs


###Custom Color in Word
This is a <#000000[0x2414]#>sample <#FFFFFFtext

<#?????? = Being Color Tag (Don't need close if is the last color change)

[0x????] = Unknown Tag parameter

\#> = End of color tag


###Unknown
this is a [0x112233...] sample text

[0x??????] (3 bytes or more of length) = Unknown command, don't edit 
