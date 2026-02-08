-- 存储过程：批量升级用户的所有Pokemon
CREATE OR ALTER PROCEDURE BulkLevelUpUserPokemon
    @UserId NVARCHAR(450),
    @LevelsToAdd INT = 1,
    @MaxLevel INT = 100
AS
BEGIN
    SET NOCOUNT ON;
    
    -- 更新所有未达到最大等级的Pokemon
    UPDATE UserPokemons 
    SET 
        Level = CASE 
            WHEN Level + @LevelsToAdd > @MaxLevel THEN @MaxLevel 
            ELSE Level + @LevelsToAdd 
        END,
        MaxHealth = MaxHealth + (@LevelsToAdd * 10),
        Health = MaxHealth + (@LevelsToAdd * 10),
        ExperienceToNextLevel = CASE 
            WHEN Level + @LevelsToAdd >= @MaxLevel THEN 0
            ELSE (Level + @LevelsToAdd) * 100
        END
    WHERE UserId = @UserId 
        AND Level < @MaxLevel;
    
    -- 返回受影响的行数
    SELECT @@ROWCOUNT as AffectedRows;
END