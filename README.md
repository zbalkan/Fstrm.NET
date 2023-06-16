# Fstrm.NET

The Fstrm.NET is a C# implementation based on the [Python implementation](https://github.com/dmachard/python-framestream).

`fstrm` is a [C implementation](https://farsightsec.github.io/fstrm/) of the Frame Streams data transport protocol.


## Protocol details

### Frame Streams Control Frame Format

Data frame length equals 00 00 00 00

| Section | Size |
|------------------------------------|----------------------|
| Data frame length                  | 4 bytes              |  
| Control frame length               | 4 bytes              |
| Control frame type                 | 4 bytes              |
| Control frame content type         | 4 bytes (optional)   |
| Control frame content type length  | 4 bytes (optional)   |
| Content type payload               | xx bytes             |     

### Frame Streams Data Frame Format

| Section | Size |
|------------------------------------|----------------------|
| Data frame length                  | 4 bytes              |
| Payload - Protobuf                 | xx bytes             |
