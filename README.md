# Bank Card Tokenization Multithreaded Server

Individual project for the OOP with C# .NET elective course during the first semester of my second year at Sofia University, FMI

Problem Statement: Given a 16-digit card number, generate a 16-digit isomorphic image (a token) of the card number that conforms to the following requirements: 
* The number of digits of the token and the card number have to be the same
* The last four digits of the token and the card number have to match
* The first 12 digits of the token have to be (pseudo-) randomly generated and must not match the corresponding digits of the card number
* The first digit of the token must not be in the set {3, 4, 5, 6}, as they are used by the major brands of bank cards
* The sum of the token's digits must not be divisible by 10
* There could be multiple valid tokens corresponding to a single valid card number

Other required features: 
* An access control system
  * Only a registered user with the required permissions can register a token
  * Only a registered user with the required permissions can request a card number (by providing a valid token)
  * Only a registered user with the required permissions could request and save a file with a table containing all cards and their corresponding tokens, sorted by token
  * Only a registered user with the required permissions could request and save a file with a table containing all cards and their corresponding tokens, sorted by card number

Note: The server does not support user registration, as that is a responsibility of another part of the system. The server only checks if a user is valid.

Sample registered <user, password> pairs:
User  Password
-----|-----
admin | Парола: 1234 – може да регистрира токени и да изисква номера на карти
user1 | Парола: 321 – не може да регистрира токени, но може да изисква номера на карти
user2 | Парола: 3211 - може да регистрира токени, но не може да изисква номера на карти
user3 | Парола: 111 – не може да регистрира токени и не може да изисква номера на карти
