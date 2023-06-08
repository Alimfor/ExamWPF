CREATE TABLE QA (
	QuestionId INT IDENTITY PRIMARY KEY,
    Question NVARCHAR(100),
    Answer NVARCHAR(100)
);

INSERT INTO QA (Question, Answer)
VALUES
    ('Сколько материков на Земле?', '6'),
    ('Сколько планет в солнечной системе?', '8'),
    ('Спутник земли', 'Луна'),
    ('День защиты детей в июне', '1'),
    ('Фамилия первого космонавта', 'Гагарин');


CREATE TABLE UserAnswers (
	id INT IDENTITY PRIMARY KEY,
    QuestionId INT,
    UserAnswer NVARCHAR(100)
);

CREATE TABLE UserInfo (
    id INT IDENTITY PRIMARY KEY,
    LastName NVARCHAR(100),
    FirstName NVARCHAR(100),
    StartDate DATE,
    EndDate DATE,
    UserAnswersId INT FOREIGN KEY REFERENCES UserAnswers(id)
);

ALTER PROCEDURE CheckUserAnswers
AS
BEGIN

    SELECT
        QA.Question AS Question,
        UA.UserAnswer AS UserAnswer,
        CASE
            WHEN UA.UserAnswer = QA.Answer THEN 'Правильно'
            ELSE 'Неправильно'
        END AS Result
    FROM
        UserAnswers UA
        JOIN QA ON UA.QuestionId = QA.QuestionId
END;
