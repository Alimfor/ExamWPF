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

CREATE TABLE UserInfo (
    UserInfoId INT IDENTITY PRIMARY KEY,
    LastName NVARCHAR(100),
    FirstName NVARCHAR(100),
    StartDate DATE,
    EndDate DATE,
    ResultInProcent INT DEFAULT 0
);

SELECT FirstName, LastName, StartDate, EndDate, ResultInProcent FROM UserInfo " +
                               "WHERE StartDate >= '2023-06-10' AND EndDate <= '2023-06-10'

SELECT * 
FROM UserInfo

CREATE TABLE UserAnswers (
	UserAnswerId INT IDENTITY PRIMARY KEY,
    QuestionId INT FOREIGN KEY REFERENCES QA(QuestionId),
    UserAnswer NVARCHAR(100),
    UserInfoId INT FOREIGN KEY REFERENCES UserInfo(UserInfoId)
);

CREATE PROCEDURE InsertQA
    @Question NVARCHAR(100),
    @Answer NVARCHAR(100)
AS
BEGIN
    IF @Question IS NULL OR @Answer IS NULL
    BEGIN
        SELECT 1 AS STATUS;
        RETURN;
    END

    INSERT INTO QA (Question, Answer)
    VALUES (@Question, @Answer);

    SELECT 0 AS STATUS;
END;

CREATE PROCEDURE InsertUserInfo
    @LastName NVARCHAR(100),
    @FirstName NVARCHAR(100),
    @StartDate DATE,
    @EndDate DATE
AS
BEGIN
    IF @LastName IS NULL OR @FirstName IS NULL OR @StartDate IS NULL OR @EndDate IS NULL
    BEGIN
        SELECT 1 AS STATUS;
        RETURN;
    END

    INSERT INTO UserInfo (LastName, FirstName, StartDate, EndDate)
    VALUES (@LastName, @FirstName, @StartDate, @EndDate);

    SELECT 0 AS STATUS;
END;


ALTER PROCEDURE InsertUserAnswers
    @QuestionId INT,
    @UserAnswer NVARCHAR(100),
    @UserInfoId INT
AS
BEGIN
    IF @QuestionId IS NULL OR @UserAnswer IS NULL OR @UserInfoId IS NULL
    BEGIN
        SELECT 1 AS STATUS;
        RETURN;
    END

    IF NOT EXISTS (SELECT 1 FROM QA WHERE QuestionId = @QuestionId)
    BEGIN
        SELECT 2 AS STATUS;
        RETURN;
    END

    IF NOT EXISTS (SELECT 1 FROM UserInfo WHERE UserInfoId = @UserInfoId)
    BEGIN
        SELECT 3 AS STATUS;
        RETURN;
    END

    INSERT INTO UserAnswers (QuestionId, UserAnswer, UserInfoId)
    VALUES (@QuestionId, @UserAnswer, @UserInfoId);

    SELECT 0 AS STATUS;
END;


ALTER PROCEDURE CheckUserAnswers
    @UserInfoId INT
AS
BEGIN
    SELECT
        QA.QuestionId AS QuestionId,
        QA.Question AS Question,
        QA.Answer AS CorrectAnswer,
        UA.UserAnswer AS UserAnswer,
        CASE
            WHEN LOWER(UA.UserAnswer) = LOWER(QA.Answer) THEN 'Правильно'
            ELSE 'Неправильно'
        END AS Result
    FROM
        UserAnswers UA
        INNER JOIN QA ON UA.QuestionId = QA.QuestionId
    WHERE
        UA.UserInfoId = @UserInfoId;
END;

