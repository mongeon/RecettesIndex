-- Une note absente et une note de zéro veulent dire la même chose dans cette app :
-- personne ne s'est prononcé. La colonne acceptait pourtant NULL sans valeur par défaut,
-- si bien qu'une insertion SQL — contrairement au formulaire, qui écrit toujours 0 —
-- laissait des NULL derrière elle.
--
-- Le code tolère désormais ces NULL (Recipe.RatingValue), mais la base ne devrait pas en
-- produire de nouveaux : une seule façon de dire « pas encore notée » vaut mieux que deux.
--
-- Re-jouable : les trois instructions sont sans effet si elles ont déjà été appliquées.

begin;

-- 1. Les lignes déjà écrites.
update recettes
set rating = 0
where rating is null;

-- 2. Ce que les prochaines insertions recevront sans le demander.
alter table recettes
alter column rating set default 0;

-- 3. Et l'interdiction d'y revenir.
alter table recettes
alter column rating set not null;

commit;
