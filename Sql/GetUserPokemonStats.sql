-- 获取用户Pokemon的统计信息
SELECT 
    COUNT(*) as TotalPokemon,
    COUNT(CASE WHEN up.IsFavorite = 1 THEN 1 END) as FavoritePokemon,
    AVG(CAST(up.Level as FLOAT)) as AverageLevel,
    MAX(up.Level) as HighestLevel,
    MIN(up.Level) as LowestLevel,
    SUM(up.BattlesWon) as TotalBattlesWon,
    SUM(up.BattlesLost) as TotalBattlesLost,
    COUNT(CASE WHEN up.Health = 0 THEN 1 END) as FaintedPokemon,
    COUNT(CASE WHEN up.Level = 100 THEN 1 END) as MaxLevelPokemon
FROM UserPokemons up
WHERE up.UserId = @UserId;