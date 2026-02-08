-- 直接的批量升级查询（不是存储过程）
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