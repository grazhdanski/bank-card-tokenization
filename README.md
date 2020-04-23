# Bank Card Tokenization Multithreaded Server and Client

Individual project for the OOP with C# .NET elective course during the first semester of my second year at Sofia University, FMI

Problem Statement: Given a 16-digit card number, generate a 16-digit isomorphic image (a token) of the card number that conforms to the following requirements: 
* The number of digits of the token and the card number have to be the same
* The last four digits of the token and the card number have to match
* The first 12 digits of the token have to be (pseudo-) randomly generated and must not match the corresponding digits of the card number
* The first digit of the token must not be in the set {3, 4, 5, 6}, as they are used by the major brands of bank cards
* The sum of the token's digits must not be divisible by 10
